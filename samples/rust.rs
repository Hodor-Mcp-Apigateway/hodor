// Hodor API - Rust sample (Linux, Mac, Windows)
// Run: cargo run --manifest-path samples/Cargo.toml
// Or: cd samples && cargo run

fn main() -> Result<(), Box<dyn std::error::Error>> {
    let base_url = std::env::var("HODOR_URL").unwrap_or_else(|_| "http://localhost:8080".to_string());

    println!("=== Hodor MCP Gateway - Rust sample ===");
    println!("Base URL: {}\n", base_url);

    let client = reqwest::blocking::Client::new();

    for path in ["/health", "/ready", "/api/tools"] {
        let url = format!("{}{}", base_url, path);
        let body = client.get(&url).send()?.text()?;
        let preview: String = body.chars().take(80).collect();
        println!("{}: {}...", path, preview);
    }

    Ok(())
}
