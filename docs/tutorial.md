# 使用教學

| Link | Description |
|---|---|
| [Install Plug-in](Tutorial.md#step1) | 手把手教你如何安裝插件 |
| [How to use](Tutorial.md#step2) | 手把手教你如何使用插件 |
| [常見問題](Tutorial.md#常見問題) | 解答各種疑難雜症 |

## Step1.

### 下載插件

至 [下載連結](https://github.com/waiwaimi/SiteModel_Killer/releases) 下載最新版本的插件，檔名應為`SiteModelKiller.gha`

![image](https://github.com/waiwaimi/SiteModel_Killer/blob/master/docs/pic/download_release.png)

### 安裝插件

1. 開啟Grasshopper後，從左上角找到 `File > Special Folders > Components Folder`

2. 將`SiteModelKiller.gha`檔案放入該資料夾

3. 右鍵檔案進入內容，將下方的 `解除封鎖` 勾選

![image](https://github.com/waiwaimi/SiteModel_Killer/blob/master/docs/pic/plugin_unlock.jpg)

4. 將Rhino關閉後重新開啟即可使用，若成功安裝則顯示如下

![image](https://github.com/waiwaimi/SiteModel_Killer/blob/master/docs/pic/install_plugin_success.jpg)

## Step2.

### 前置作業

1. 若曲線或文字不在同一個平面上，請先壓平(建議使用`SetPt`)

2. 隱藏 街廓線圖層、圖塊(空地、廟宇、停車場之類的，文字不必關閉)，如果你需要的曲線跟文字在同一個圖塊或群組中，請先炸開(所有線都炸開的話生成品質可以更上一層樓)

3. 建立一個新圖層，用來放生成出的3D模型

### 生成量體

(這裡屬於**從0開始使用Grasshopper**，如果你本來就會使用Grasshopper，可以直接跳過這裡，或是直接參考[範例檔](https://github.com/waiwaimi/SiteModel_Killer/blob/master/Example_Files)使用)

如果你很真的很不會用Grasshopper，請先在[這裡](https://github.com/waiwaimi/SiteModel_Killer/blob/master/Example_Files)下載對應版本的`.gh`檔案

1. 開啟Grasshopper

2. 在Grasshopper開啟舊檔 > 開啟剛剛下載的`.gh`檔

3. 回到Rhino，選取所有生成需要的文字(可以使用`SelText`，或是事先放到同一個圖層再選取)，再回到Grasshopper，右鍵`標示(文字)`電池 > Set Multiple Guids

![image](https://github.com/waiwaimi/SiteModel_Killer/blob/master/docs/pic/use_SetMultipleGuids.jpg)

4. 再次回到Rhino，選取所有要生成量體的曲線，加入至`建築量體邊界`(跟第3步驟差不多)

5. 建立或選取一個生成範圍的框框(意思是不管原檔範圍多大，只會生成這個框框內範圍的量體)，加入至`底座(線框或平面)`

6. 在`FloorHeight`設定每層樓高(請注意單位!)

(如果不小心選錯曲線了請參考[這裡](Tutorial.md#選錯東西了怎麼辦))

7. 選取`Brep`電池 > Bake

![image](https://github.com/waiwaimi/SiteModel_Killer/blob/master/docs/pic/use_BakeBuilding.jpg)

### 生成女兒牆

1. 選取要生成女兒牆的量體(多重曲面)，加入至`量體`(步驟與上面相同)

2. 調整女兒牆高度、厚度

3. 選擇是否包含量體(True代表Bake之後會有光頭量體+長頭髮量體，建議Bake在與上一步驟不同圖層內，再將原本圖層內的量體刪除)

4. 選取`Brep`電池 > Bake

## 常見問題

#### 按照步驟安裝Plug-in卻沒有顯示在Grasshopper

- 本插件僅支援Rhino 7，請先確認自己的Rhino版本

- 確認是否有勾選`解除封鎖`

- 確認`View > Component Tabs`有開啟顯示

#### 選錯東西了怎麼辦

選取電池後右鍵 > `Clear Values`

#### 量體生成到不該生成的地方怎麼辦

- 確認曲線都有交會

- 確認文字基準點都有在量體內

- 確認所有物件都在同一平面上

以上可以在調整過後`Clear Values`重新設定，或是Bake之後再將不需要的物件刪除並單獨重新生成需要的量體

#### 有些量體Bake之後顯示怪怪的(變成半透明)怎麼辦

這個是面反轉的問題，建議將原本的曲線炸開後再生成一次即可，但其實不影響模型，不嫌棄的話也可以不用理會

#### 看完教學我還是不會用Grasshopper怎麼辦

本專案不負責教學Grasshopper，請自行上網查詢或是詢問身旁同學

這邊提供推薦的Youtube影片(中文) [Grasshopper入門教學](https://www.youtube.com/watch?v=6Skj2kn49J8&list=PLbVdFUxIVAqKLZH0ima63kPLIcW0viWV_)