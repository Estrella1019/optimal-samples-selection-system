#!/bin/bash
# ── Optimal Samples Selection System — Start Script ──────────────────────
# Usage: bash start.sh

cd "$(dirname "$0")"

# Create venv if needed
if [ ! -d "venv" ]; then
  echo "Setting up virtual environment..."
  python3 -m venv venv
fi

# Activate and install deps
source venv/bin/activate
pip install flask -q

# Show LAN IP so mobile devices can connect
echo ""
echo "================================================"
echo "  Optimal Samples Selection System"
echo "  Local:  http://localhost:8080"
LAN_IP=$(ipconfig getifaddr en0 2>/dev/null || ipconfig getifaddr en1 2>/dev/null)
if [ -n "$LAN_IP" ]; then
  echo "  Mobile: http://$LAN_IP:8080"
fi
echo "  (Open the above URL in your browser)"
echo "  Press Ctrl+C to stop"
echo "================================================"
echo ""

python3 app.py
