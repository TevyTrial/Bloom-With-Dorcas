# Equipped Item Debug System - Setup & Usage Guide

## 🔧 What Was Added

### 1. **Comprehensive Logging System**
All equipped item operations now log with stack traces:

- ✅ When items are equipped (`EquipHandSlot`)
- ✅ When equipped items are replaced
- ✅ When slots are emptied (`Empty()`)
- ✅ When items are moved via drag-and-drop
- ✅ When items are moved via click

### 2. **Protection Mechanisms**

#### HandInventorySlot.cs
- **OLD BUG**: `OnPointerEnter` would move equipped items just by hovering
- **FIX**: Now requires explicit click to unequip
- **Protection**: Blocks operations during drag events

#### InventoryBox.cs
- **Protection**: `OnPointerEnter` now ignores events during drag operations
- **Protection**: Added logging to all click events

#### InventorySlotDragHandler.cs
- **Protection**: Cannot drag to same slot
- **Logging**: Full trace of all drag operations (source → target)
- **Logging**: Shows equipped vs inventory operations clearly

### 3. **Real-time Monitor (EquippedItemMonitor.cs)**
Live debugging tool that tracks equipped item changes

---

## 🚀 How to Use

### Step 1: Add the Monitor
1. In Unity, create an empty GameObject: `GameObject > Create Empty`
2. Name it "EquippedItemDebugMonitor"
3. Add component: `EquippedItemMonitor`
4. Check "Enable Logging" in Inspector
5. Adjust `Check Interval` if needed (default 0.5s)

### Step 2: Test Your Game
Play your game normally and watch the Console window

### Step 3: Look for These Log Patterns

#### ✅ **Normal Equip Operation**
```
[InventoryManager] EquipHandSlot: equipping 'Watering Can' as Tool
[InventoryManager] RenderEquippedItem: ...
```

#### ⚠️ **Suspicious Unequip** (Bug!)
```
[EquippedMonitor] TOOL SLOT CHANGED: 'Watering Can' -> 'null'
Stack Trace:
   at InventoryBox.OnPointerEnter(...)
   at UnityEngine.UI.EventSystem...
```
This tells you **exactly** what code cleared the equipped item!

#### ⚠️ **Unexpected Empty Call** (Bug!)
```
[ItemSlotData] Emptying slot with item 'Watering Can' - Stack Trace:
   at ItemSlotData.Empty()
   at SomeUnexpectedMethod()
```

---

## 🐛 Common Bug Patterns to Look For

### Pattern 1: OnPointerEnter Bug
**Symptom**: Item unequips when hovering over UI
**Log**: Stack trace shows `OnPointerEnter` in the trace
**Cause**: Fixed in `HandInventorySlot.cs` - was triggering on hover
**Solution**: Already applied - now requires click

### Pattern 2: Drag Operation Bug
**Symptom**: Item disappears after dragging other items
**Log**: Look for `[DragHandler]` logs without matching target
**Cause**: Drag might be affecting equipped slot
**Solution**: Check `isEquippedSlot` flag is set correctly in Inspector

### Pattern 3: UI Refresh Bug
**Symptom**: Item disappears when inventory updates
**Log**: `[EquippedMonitor]` shows change during `RenderInventory`
**Cause**: UI refresh might be recreating slots incorrectly
**Solution**: Check UIManager's inventory rendering code

### Pattern 4: Stacking Bug
**Symptom**: Equipped item vanishes when stacking similar items
**Log**: Check for `AddQuantity` followed by `Empty()` on equipped slot
**Cause**: Stacking logic might be targeting wrong slot
**Solution**: Ensure equipped slots are excluded from stacking operations

---

## 🎯 Debug Checklist

When you see the bug happen:

1. ✅ Check Console - what was the last log before item disappeared?
2. ✅ Look at Stack Trace - which method triggered `Empty()`?
3. ✅ Check on-screen monitor - did Tool or Item slot change?
4. ✅ Note what you were doing:
   - [ ] Dragging items?
   - [ ] Clicking inventory slots?
   - [ ] Opening/closing inventory?
   - [ ] Picking up new items?
   - [ ] Just standing idle?

5. ✅ Look for these specific log entries:
   ```
   [DragHandler] PerformDrop START
   [InventoryBox] OnPointerClick
   [HandInventorySlot] OnPointerClick
   [ItemSlotData] Emptying slot
   ```

---

## 🔍 How to Read Stack Traces

Example stack trace:
```
[ItemSlotData] Emptying slot with item 'Hoe' - Stack Trace:
   at ItemSlotData.Empty() in ItemSlotData.cs:line 70
   at InventorySlotDragHandler.PerformDrop() in InventorySlotDragHandler.cs:line 195
   at InventorySlotDragHandler.OnDrop() in InventorySlotDragHandler.cs:line 127
```

**Reading**: 
- Item 'Hoe' was cleared
- From: `InventorySlotDragHandler.PerformDrop` at line 195
- Triggered by: `OnDrop` event
- **Action**: Check line 195 in InventorySlotDragHandler.cs

---

## 🛠️ Inspector Setup Checklist

### For Equipped Slot UI Elements:
- [ ] Has `InventorySlotDragHandler` component
- [ ] `Is Equipped Slot` checkbox is **CHECKED**
- [ ] `inventoryBox` reference assigned
- [ ] `itemIcon` reference assigned
- [ ] `canvasGroup` will auto-create

### For Regular Inventory Slots:
- [ ] Has `InventorySlotDragHandler` component
- [ ] `Is Equipped Slot` checkbox is **UNCHECKED**
- [ ] `inventoryBox` reference assigned
- [ ] `itemIcon` reference assigned

### For Hand Slot Display:
- [ ] Uses `HandInventorySlot` component (NOT regular `InventoryBox`)
- [ ] `boxType` set correctly (Tool or Item)

---

## 📊 Performance Notes

The `EquippedItemMonitor` checks every 0.5 seconds by default. This is lightweight but you can:
- Increase interval to 1.0s for less frequent checks
- Decrease to 0.1s for more detailed tracking
- Disable after finding the bug to improve performance

---

## 🎬 Next Steps

1. **Play your game** with the monitor active
2. **Reproduce the bug** where items disappear
3. **Check the Console** immediately when it happens
4. **Find the stack trace** that shows what cleared the item
5. **Report back** with the log output so we can fix the specific cause

The logs will tell us EXACTLY what code is clearing your equipped items!
