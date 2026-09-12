# Changelog

## 1.0.2

- Translated all in-game notifications, configuration descriptions, and log messages into English.
- Added a packaging check to prevent the previously used Portuguese runtime phrases from returning.
- Established English as the required language for public documentation and user-facing text.
- Refocused the package README on players and moved contributor details to `DEVELOPMENT.md`.

## 1.0.1

- Fixed patch loading for `InventoryGrid.OnLeftDown` on Valheim 1.0.7.
- Harmony now identifies method arguments by position instead of relying on internal names.

## 1.0.0

- Added Alt + left-click to toggle favorite state in the player inventory.
- Added a subtle golden star to favorite slots.
- Added persistence through `ItemData.m_customData` without preventing vanilla stack merging.
- Preserved favorite state safely when splitting and merging stacks.
- Added quick-stack protection for GorilaChestMod 2.2.2.
- Added optional automatic protection for EquipmentAndQuickSlots 3.x slots.
