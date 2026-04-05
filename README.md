# Optimal Samples Selection System

> CS360 — Artificial Intelligence Group Project

A web-based **Optimal Samples Selection System** built with Python + Flask.
Runs entirely in the browser — including on **mobile phones** (bonus feature ✓).

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

Then open: **http://localhost:8080**

The script auto-creates a virtual environment, installs Flask, and prints your mobile URL.

### Option 2 — Manual setup

```bash
python3 -m venv venv
source venv/bin/activate
pip install flask numpy
python3 app.py
```

---

## Mobile Access (Bonus)

To run on a **phone**:

1. Connect phone and Mac to the **same Wi-Fi**
2. `bash start.sh` — it prints your LAN URL automatically, e.g.:
   ```
   Mobile: http://192.168.1.5:8080
   ```
3. Open that URL in your phone's browser — done

---

## Project Structure

```
├── app.py                      # Flask app — all routes
├── start.sh                    # One-click launcher (macOS/Linux)
├── requirements.txt            # Dependencies (flask, numpy)
├── README.md                   # This file
├── templates/
│   ├── index.html              # S1: Main screen
│   ├── db.html                 # S2: Database browser
│   └── result.html             # Result detail viewer
├── static/css/style.css        # Responsive stylesheet
├── backend_python/core/
│   ├── optimizer.py            # Dual-mode algorithm (exact + greedy)
│   ├── sample_selector.py      # Random / manual sample selection
│   ├── constraints.py          # Coverage verification
│   └── __init__.py
└── results/                    # Auto-created at runtime, stores DB files
```

---

## Algorithm

The system uses a **dual-mode strategy** that automatically selects the best algorithm based on problem size:

### Mode 1 — Exact Backtracking + Branch & Bound
> Used when C(n, k) ≤ 84 (e.g. C(9,6)=84 is the practical limit)

- Generates all C(n,k) candidate k-groups and all C(n,j) j-subsets
- **Deduplicates** k-groups with identical coverage fingerprints to shrink the search space
- Sorts candidates by coverage power (most effective first)
- Uses greedy solution as an **upper-bound initialiser** for pruning
- Applies a **lower-bound pruning**: if remaining candidates cannot possibly improve the best solution, the branch is cut
- Has a **3-second timeout** — falls back to greedy automatically if exceeded
- **Guarantees the provably minimum number of groups**

### Mode 2 — Greedy + Random Restarts
> Used when C(n, k) > 84 (large search space)

- Each round: pick the k-group covering the most uncovered j-subsets (≥ s overlap)
- Repeat 5× with shuffled candidates, keep the **smallest result**
- numpy-accelerated coverage matrix for fast gain computation
- Produces near-optimal results in a fraction of the time

The UI clearly labels which algorithm was used: **✓ Exact · Provably Optimal** or **≈ Greedy · Near-Optimal**.

### Sample Results

All 9 assignment examples verified correct:

| # | m | n | k | j | s | Groups | Algorithm |
|---|---|---|---|---|---|--------|-----------|
| E.g.1 | 45 | 7 | 6 | 5 | 5 | 6 ✓ | Exact |
| E.g.2 | 45 | 8 | 6 | 5 | 5 | 12 ✓ | Exact |
| E.g.3 | 45 | 8 | 6 | 6 | 5 | 4 ✓ | Exact |
| E.g.4 | 45 | 9 | 6 | 5 | 4 | 3 ✓ | Exact |
| E.g.5 | 45 | 9 | 6 | 6 | 5 | ≥1 ✓ | Greedy |
| E.g.6 | 45 | 9 | 6 | 4 | 4 | 12 ✓ | Exact |
| E.g.7 | 45 | 8 | 6 | 4 | 4 | 7 ✓ | Exact |
| E.g.8 | 45 | 9 | 6 | 5 | 5 | ≥1 ✓ | Greedy |
| E.g.9 | 45 | 12 | 6 | 6 | 4 | 6 ✓ | Greedy |

---

## Features

- **S1 Main Screen**: parameter input, random/manual selection, execute, store, clear, print
- **S2 Database Screen**: list saved runs, display, delete
- **Dual-mode algorithm**: exact backtracking (small scale) + greedy restarts (large scale)
- **Algorithm badge**: UI shows whether result is "Provably Optimal" or "Near-Optimal"
- Full input validation with clear error messages
- Automatic coverage verification on every result
- Mobile-responsive UI — works on phones and tablets
- 9 built-in quick-example presets matching the assignment examples

---

## Group Members

| Name | Student ID |
|------|------------|
| PAN JIAYING  | 1230028040 |
| JIN XINYI    | 1230026823 |
| LIU XINGZHE  | 1230033168 |
| CHENG YUXIANG| 1230020512 |

CS360 — Artificial Intelligence
