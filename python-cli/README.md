# Grocery Sort CLI Prototype

Grocery Sort CLI Prototype is the first version of the Grocery Sort project, built in Python as a command-line application.

The program helps users organize grocery lists based on predefined store layouts. Grocery items are automatically sorted according to the order in which store sections are typically visited during shopping.

This prototype served as the foundation for later Grocery Sort projects, including the WPF desktop application.

---

## Features

**Store Selection**
- Choose between predefined store layouts
- Currently supports:
  - Billa
  - Spar

**Grocery Input**
- Add grocery items manually
- Assign each item to a store section

**Automatic Sorting**
- Grocery items are sorted according to store layout order
- Creates a more efficient shopping flow

**Input Validation**
- Detects invalid  store selection
- Prevents invalid section numbers
- Handles incorrect numeric input

**List Saving**
- Optional JSON-based saving
- Lists are stored by date
- Multiple lists per day supported

---

## Technologies Used
- Python
- JSON
- Command Line Interface (CLI)

---

## Requirements
- Python 3.x

---

## How to Use

1. Run the script
2. Choose a store layout
3. Enter grocery items
4. Assign each item to a store section
5. View the sorted grocery list
6. Optionally save the list as JSON

Run with:
```bash
python grocery_sort.py
```

---

## Goal

The goal of this prototype was to explore the core grocery sorting algorithm and validate the concept before moving to more advanced implementations with graphical user interfaces and database integration.

---

## Author

Created by Feuermelda