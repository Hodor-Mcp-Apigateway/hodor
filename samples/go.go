// Hodor API - Go sample (Linux, Mac, Windows)
// Run: go run go.go

package main

import (
	"encoding/json"
	"fmt"
	"net/http"
	"os"
)

func main() {
	baseURL := os.Getenv("HODOR_URL")
	if baseURL == "" {
		baseURL = "http://localhost:8080"
	}

	fmt.Println("=== Hodor MCP Gateway - Go sample ===")
	fmt.Printf("Base URL: %s\n\n", baseURL)

	for _, path := range []string{"/health", "/ready", "/api/tools"} {
		resp, err := http.Get(baseURL + path)
		if err != nil {
			fmt.Printf("%s: error: %v\n", path, err)
			continue
		}
		defer resp.Body.Close()
		var v interface{}
		json.NewDecoder(resp.Body).Decode(&v)
		fmt.Printf("%s: %v\n", path, v)
	}
}
