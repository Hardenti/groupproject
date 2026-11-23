# Wireframes

## Pantry dashboard

```text
+ Pantry Table       Dashboard  Inventory  Donations  Distributions  Households  Food catalog  Donors
|
|  Good food, shared well.                               [Record donation] [Prepare distribution]
|  Track donations, protect fresh stock...
|
|  Active items       Households       Low stock       Expiring soon
|       8                 42               3                2
|
|  Items to keep an eye on                 7-day weather / pickup advisory
|  Food       On hand   Expiry             Day        High     Rain %
|  Apples       3 lb    Nov 28             Thu        21 C       70%
|
|  Recent household distributions
|  Household             Date              Item lines
+---------------------------------------------------------------------------
```

## Donation intake

```text
Receive a donation
Donor [select]                    Receiving pantry [select]
Received date [date]
Items received
Food item                  Quantity              Best before
Apples (lb)                [____]                [date]
Rice (lb)                  [____]                [optional]
Donation notes [____________________________________________]
[Save donation and update stock]  [Cancel]
```

## Distribution issue

```text
Prepare distribution
Household [select]                Pantry site [select]
Date/time [datetime]
Items to provide                  Available       Quantity to issue
Apples (lb)                         20 lb              [____]
Rice (lb)                           50 lb              [____]
Beans (can)                         48 can             [____]
Notes [____________________________________________]
[Save distribution] [Cancel]
```

## Accessibility and tab flow

Forms use visible labels, native controls, keyboard-operable buttons, server-side validation, and responsive tables. The logical tab sequence follows the visual order: page actions, search/filter, table links/actions, then form controls from top to bottom. Errors appear near fields and in a summary.
