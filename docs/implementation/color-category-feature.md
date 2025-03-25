# 分類顏色功能實施計劃

## 功能需求
1. 為每個分類添加自定義顏色碼
2. 在日曆視圖中顯示分類對應顏色
3. 提供15個預設顏色選擇
4. 允許自定義顏色選擇

## 技術方案

### 後端修改
1. **Category 模型修改**
```csharp
public class Category
{
    // 新增屬性
    public string ColorCode { get; set; } = "#3A86FF"; // 預設藍色
}
```

2. **數據庫遷移**
```bash
Add-Migration AddColorCodeToCategory
Update-Database
```

3. **種子數據更新**
```csharp
modelBuilder.Entity<Category>().HasData(
    new Category { Id=1, Name="Entertainment", ColorCode="#E63946" },
    // 其他分類顏色...
);
```

### 前端修改
1. **顏色選擇器組件**
```html
<div class="color-picker">
  <input type="color" v-model="selectedColor">
  <div class="preset-colors">
    <div 
      v-for="color in presetColors"
      :style="{backgroundColor: color}"
      @click="selectColor(color)"
    ></div>
  </div>
</div>
```

2. **預設顏色配置 (15色)**
```javascript
presetColors: [
  '#E63946', '#F28C28', '#FFBE0B', '#70B77E', '#00A8E8',
  '#3A86FF', '#6A4C93', '#FFB6B9', '#8ECAE6', '#95D5B2',
  '#F7D6E0', '#1D3557', '#2D3A3A', '#5E503F', '#6C757D'
]
```

3. **日曆顏色顯示邏輯**
```javascript
function getCategoryColor(categoryId) {
  const category = categories.value.find(c => c.id === categoryId);
  return category?.colorCode || '#808080';
}
```

## 實施步驟
1. 後端模型更新 ✅
2. 數據庫遷移執行 ✅
3. 前端顏色選擇器實作 ✅
4. 日曆視圖整合 ✅
5. 測試與驗收 ✅
