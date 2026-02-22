#!/usr/bin/env ruby
# Hodor API - Ruby sample (Linux, Mac, Windows)
# Run: ruby ruby.rb
# Requires: net/http, json (stdlib)

require 'net/http'
require 'json'

base_url = ENV['HODOR_URL'] || 'http://localhost:8080'

puts "=== Hodor MCP Gateway - Ruby sample ==="
puts "Base URL: #{base_url}\n\n"

%w[/health /ready /api/tools].each do |path|
  uri = URI("#{base_url}#{path}")
  res = Net::HTTP.get_response(uri)
  data = JSON.parse(res.body) rescue res.body
  puts "#{path}: #{data.to_s[0..150]}..."
end
