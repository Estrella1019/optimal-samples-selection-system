# Optimal Samples Selection System

> CS360 — Artificial Intelligence Group Project

A web-based **Optimal Samples Selection System** built with Python + Flask.
Runs entirely in the browser — including on **mobile phones** (bonus feature).

---

## Platform

**Developed on macOS.** No Windows required.

| Item | Detail |
|------|--------|
| OS | macOS (developed & tested) |
| Language | Python 3 |
| Framework | Flask |
| Frontend | HTML + CSS + Vanilla JS |
| Database | File-based (`.txt`, named `m-n-k-j-s-runId-count`) |

---

## What It Does

Given parameters **m, n, k, j, s**, the system:

1. Selects **n** samples from **m** total (randomly or manually entered)
2. Finds the **minimum number of k-groups** such that every j-subset of the n samples is covered by at least one k-group sharing ≥ s elements
3. Stores results to a local file database and lets you browse, display, delete, and print them

### Parameter Ranges

| Parameter | Range | Meaning |
|-----------|-------|---------|
| m | 45 ≤ m ≤ 54 | Total samples |
| n | 7 ≤ n ≤ 25 | Samples selected from m |
| k | 4 ≤ k ≤ 7 | Size of each result group |
| j | s ≤ j ≤ k | Size of j-subsets to cover |
| s | 3 ≤ s ≤ 7 | Minimum overlap required |

---

## Quick Start

### Option 1 — One command (recommended)

```bash
bash start.sh
```

Then open: **http://localhost:5000**

The script auto-creates a virtual environment, installs Flask, and prints your mobile URL.

### Option 2 — Manual setup

```bash
python3 -m venv venv
source venv/bin/activate
pip install flask
python3 app.py
```

---

## Mobile Access (Bonus)

To run on a **phone**:

1. Connect phone and Mac to the **same Wi-Fi**
2. `bash start.sh` — it prints your LAN URL automatically, e.g.:
   ```
   Mobile: http://192.168.1.5:5000
   ```
3. Open that URL in your phone's browser — done

---

## Project Structure

```
├── app.py                      # Flask app — all routes
├── start.sh                    # One-click launcher (macOS/Linux)
├── requirements.txt            # Dependencies (flask only)
├── templates/
│   ├── index.html              # S1: Main screen
│   ├── db.html                 # S2: Database browser
│   └── result.html             # Result detail viewer
├── static/css/style.css        # Responsive stylesheet
├── backend_python/
│   └── core/
│       ├── optimizer.py        # Core algorithm
│       ├── sample_selector.py  # Sample selection
│       └── constraints.py      # Coverage verification
└── results/                    # Auto-created, stores DB files
```

---

## Algorithm

Greedy covering with **random restarts**:

- Generate all k-subsets from the n selected samples
- Each round: pick the k-subset covering the most uncovered j-subsets (≥ s overlap)
- **Frequency-based tiebreaking**: prefer elements appearing most in uncovered subsets
- Repeat 10× with shuffled candidates, keep the **smallest result**

### Sample Results

| m | n | k | j | s | Groups |
|---|---|---|---|---|--------|
| 45 | 7 | 6 | 5 | 5 | 6 ✓ |
| 45 | 8 | 6 | 5 | 5 | 12 ✓ |
| 45 | 8 | 6 | 4 | 4 | 7 ✓ |
| 45 | 9 | 6 | 4 | 4 | 12 ✓ |
| 45 | 8 | 6 | 6 | 5 | 4 ✓ |
| 45 | 9 | 6 | 5 | 4 | 3 ✓ |

---

## Features

- S1 Main Screen: parameter input, random/manual selection, execute, store, clear, print
- S2 Database Screen: list saved runs, display, delete
- Full input validation with clear error messages
- Automatic coverage verification on every result
- Mobile-responsive UI
- 8 built-in quick-example presets from the assignment

---

## Group Members

| Name | Student ID |
|------|------------|
| PAN JIAYING  |1230028040 |
| JIN XINYI    |1230026823 |
| LIU XINGZHE  |1230033168 |
| CHENG YUXIANG|1230020512 |

CS360  — Artificial Intelligence
