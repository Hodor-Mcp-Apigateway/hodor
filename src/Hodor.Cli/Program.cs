using System.CommandLine;
using System.Diagnostics;

var root = FindRepoRoot();
var url = Environment.GetEnvironmentVariable("HODOR_URL") ?? "http://localhost:8080";

var installCmd = new Command("install", "Install Hodor (Docker Compose)");
installCmd.SetHandler(async () => await InstallAsync(root, url));

var deployCmd = new Command("deploy", "Deploy Hodor");
var dockerOpt = new Option<string>("--target", "docker|helm|kind") { IsRequired = false };
deployCmd.AddOption(dockerOpt);
deployCmd.SetHandler(async (target) => await DeployAsync(root, target ?? "docker"), dockerOpt);

var healthCmd = new Command("health", "Check Hodor health");
healthCmd.SetHandler(async () => await HealthAsync(url));

var scaleCmd = new Command("scale", "Scale Hodor replicas (Docker Compose)");
var replicasOpt = new Option<int>("--replicas", () => 2, "Number of replicas");
scaleCmd.AddOption(replicasOpt);
scaleCmd.SetHandler(async (replicas) => await ScaleAsync(root, replicas), replicasOpt);

var rootCmd = new RootCommand("Hodor MCP Gateway CLI");
rootCmd.AddCommand(installCmd);
rootCmd.AddCommand(deployCmd);
rootCmd.AddCommand(healthCmd);
rootCmd.AddCommand(scaleCmd);

return await rootCmd.InvokeAsync(args);

static string FindRepoRoot()
{
    var dir = AppContext.BaseDirectory;
    while (!string.IsNullOrEmpty(dir))
    {
        if (Directory.Exists(Path.Combine(dir, "deployment", "docker")) &&
            File.Exists(Path.Combine(dir, "Hodor.slnx")))
            return dir;
        dir = Path.GetDirectoryName(dir);
    }
    return Path.GetFullPath(".");
}

static async Task<int> RunAsync(string fileName, string args, string workingDir = ".")
{
    var psi = new ProcessStartInfo
    {
        FileName = OperatingSystem.IsWindows() ? fileName + ".exe" : fileName,
        Arguments = args,
        WorkingDirectory = workingDir,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    var process = Process.Start(psi);
    if (process == null) return -1;
    await process.WaitForExitAsync();
    return process.ExitCode;
}

static async Task<(int ExitCode, string StdOut)> RunWithOutputAsync(string fileName, string args, string workingDir = ".")
{
    var psi = new ProcessStartInfo
    {
        FileName = OperatingSystem.IsWindows() ? fileName + ".exe" : fileName,
        Arguments = args,
        WorkingDirectory = workingDir,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    var process = Process.Start(psi);
    if (process == null) return (-1, "");
    var outTask = process.StandardOutput.ReadToEndAsync();
    await process.WaitForExitAsync();
    var stdout = await outTask;
    return (process.ExitCode, stdout);
}

static bool Exists(string cmd) => RunAsync(cmd, "--version").GetAwaiter().GetResult() == 0;

static async Task InstallAsync(string root, string url)
{
    Console.WriteLine("=== Hodor MCP Gateway - Install ===\n");
    if (!Exists("docker"))
    {
        Console.WriteLine("Error: Docker required. Install: https://docs.docker.com/get-docker/");
        return;
    }
    var compose = Path.Combine(root, "deployment", "docker", "docker-compose.yaml");
    var minimal = Path.Combine(root, "deployment", "docker", "docker-compose-minimal.yaml");
    var file = File.Exists(compose) ? compose : minimal;
    Console.WriteLine("[1/3] Starting Hodor...");
    await RunAsync("docker", $"compose -f \"{file}\" up -d --build", root);
    Console.WriteLine("[2/3] Waiting for Hodor...");
    for (var attempt = 1; attempt <= 10; attempt++)
    {
        if (await CheckHealthAsync(url)) { Console.WriteLine("  Ready!"); break; }
        Console.WriteLine($"  Waiting... ({attempt}/10)");
        await Task.Delay(2000);
    }
    Console.WriteLine("[3/3] Verifying...");
    await HealthAsync(url);
    Console.WriteLine("\n=== Hodor installed ===\n");
    Console.WriteLine($"  Health:   {url}/health");
    Console.WriteLine($"  Claude:   claude mcp add --scope user --transport sse hodor {url}/sse\n");
}

static async Task DeployAsync(string root, string target)
{
    Console.WriteLine($"=== Deploy: {target} ===\n");
    if (target == "docker")
    {
        var compose = Path.Combine(root, "deployment", "docker", "docker-compose.yaml");
        await RunAsync("docker", $"compose -f \"{compose}\" up -d --build", root);
        Console.WriteLine("\n  Health: http://localhost:8080/health");
    }
    else if (target == "helm")
    {
        if (!Exists("helm")) { Console.WriteLine("Error: helm required."); return; }
        var chart = Path.Combine(root, "deployment", "helm", "hodor");
        await RunAsync("helm", $"upgrade --install hodor \"{chart}\" --set autoscaling.enabled=true", root);
        Console.WriteLine("\n  kubectl get pods -l app.kubernetes.io/name=hodor");
    }
    else if (target == "kind")
    {
        if (!Exists("kind")) { Console.WriteLine("Error: kind required."); return; }
        var cluster = Environment.GetEnvironmentVariable("KIND_CLUSTER_NAME") ?? "hodor-cluster";
        Console.WriteLine("  Building image...");
        await RunAsync("docker", "build -t hodor-mcp-gateway:latest .", root);
        var (_, clustersOut) = await RunWithOutputAsync("kind", "get clusters", root);
        var clusterExists = clustersOut.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Any(line => line.Trim().Equals(cluster, StringComparison.OrdinalIgnoreCase));
        if (!clusterExists)
        {
            var config = Path.Combine(root, "deployment", "kind", "kind-config.yaml");
            var createArgs = File.Exists(config)
                ? $"create cluster --name {cluster} --config \"{config}\""
                : $"create cluster --name {cluster}";
            Console.WriteLine("  Creating Kind cluster...");
            await RunAsync("kind", createArgs, root);
        }
        else
        {
            Console.WriteLine("  Loading image into Kind...");
        }
        await RunAsync("kind", $"load docker-image hodor-mcp-gateway:latest --name {cluster}", root);
        await RunAsync("kubectl", "create namespace hodor", root);
        var chart = Path.Combine(root, "deployment", "helm", "hodor");
        await RunAsync("helm", $"upgrade --install hodor \"{chart}\" -n hodor --set image.repository=hodor-mcp-gateway --set image.tag=latest --set image.pullPolicy=IfNotPresent --set autoscaling.enabled=true", root);
        await RunAsync("kubectl", "rollout status deployment/hodor-hodor -n hodor --timeout=120s", root);
        Console.WriteLine("\n  kubectl port-forward svc/hodor-hodor 8080:8080 -n hodor");
    }
    Console.WriteLine("\n=== Done ===\n");
}

static async Task HealthAsync(string url)
{
    try
    {
        using var hc = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        var response = await hc.GetAsync($"{url.TrimEnd('/')}/health");
        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"  Status: {response.StatusCode}");
        Console.WriteLine($"  Body: {body}");
    }
    catch (Exception exception)
    {
        Console.WriteLine($"  Error: {exception.Message}");
    }
}

static async Task<bool> CheckHealthAsync(string url)
{
    try
    {
        using var hc = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var response = await hc.GetAsync($"{url.TrimEnd('/')}/health");
        return response.IsSuccessStatusCode;
    }
    catch { return false; }
}

static async Task ScaleAsync(string root, int replicas)
{
    var compose = Path.Combine(root, "deployment", "docker", "docker-compose.yaml");
    await RunAsync("docker", $"compose -f \"{compose}\" up -d --scale hodor={replicas}", root);
    Console.WriteLine($"  Scaled to {replicas} replicas");
}
