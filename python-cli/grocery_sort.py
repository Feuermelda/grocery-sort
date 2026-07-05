import json
import datetime
import os


billa_layout = [
    "Fruits",
    "Vegetables",
    "Pasta, Tomato Cans and Rice",
    "Refrigerated Shelf",
    "Meat and Cold Cuts",
    "Eggs and Dairy",
    "Bakery",
    "Snacks",
    "Beverages",
    "Pantry",
    "Frozen",
    "Sweets",
    "Home Essentials",
    "Other",
]

spar_layout = [
    "Bene's Tasks",
    "Fruits",
    "Vegetables",
    "Refrigerated Shelf",
    "Dairy",
    "Meat and Cold Cuts",
    "Fish",
    "Pantry",
    "Bakery",
    "Sweets",
    "Snacks",
    "Frozen",
    "Other",
]


def sort_grocery_dict(store_layout, grocery_dict):
    sorted_grocery_list = []
    for store_section in store_layout:
        for fooditem, item_section in grocery_dict.items():
            if item_section == store_section:
                sorted_grocery_list.append(fooditem)

    return sorted_grocery_list


while True:
    store = (
        input(
            "Please choose name of store:\n\n[B] Billa\n[S] Spar\n").lower().strip()
    )

    if store not in ["b", "s"]:
        print("Invalid Input. Please Try again.\n")
        continue
    elif store == "b":
        layout = billa_layout
        break

    elif store == "s":
        layout = spar_layout
        break

print("\nThese are all available Sections:")
for index, section in enumerate(layout, start=1):
    print(f"{index}. {section}")

print("\nTo stop adding items press Enter...\n")

grocery_list = {}

while True:
    product = input("Enter product: ")
    if product == "":
        break
    try:
        sect = int(input("Enter section number: "))
        if sect not in range(1, len(layout)+1):
            print("\nInvalid Section Number.\n")
        else:
            grocery_list.update({product: layout[sect - 1]})

    except ValueError:
        print("\nOops, thats not a number. Enter only integers please.\n")
        continue

grocery_list_sorted = sort_grocery_dict(layout, grocery_list)

print("\nSorted Grocery List:\n")
for item in grocery_list_sorted:
    print(item)

print("\n")

save_list = input(
    "Want to save this list?\nY for Yes\nN for No\n").lower().strip()

filename = "sorted_grocery_lists.json"
date_key = str(datetime.date.today())

if save_list == "y":

    if os.path.exists(filename):
        try:
            with open("sorted_grocery_lists.json", "r", encoding="utf-8") as f:

                data = json.load(f)

        except json.JSONDecodeError:
            # file exists but is empty / corrupted - start new
            data = {}
    else:
        data = {}

    # Add / append today's list
    if date_key in data:
        # already saved something today -> append as a new entry
        if isinstance(data[date_key], list):
            data[date_key].append(grocery_list_sorted)
        else:
            # convert single list to list-of-lists first
            data[date_key] = [data[date_key], grocery_list_sorted]
    else:
        data[date_key] = [grocery_list_sorted]

    # overwrite the file with the updated JSON
    with open(filename, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=4)

    print("\nGrocery List saved.\n")

else:
    print("\nGrocery List not saved.\n")

print("Have a pleasant shopping trip!\n")
