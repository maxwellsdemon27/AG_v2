# AG重要程式碼位置

###### tags: `AG`

## 專案組成
- 本gitlab主要控管核心程式碼的版本，整個AG專案壓縮存放路徑在`nfs://storage.ai.icrd/nfs/rltsp/AG/AG_final.7z`
- 專案資料夾內含有許多unity所使用的物件，而避障、攻擊路徑、船團陣型推估等演算法邏輯則存放在`/AG_final/GF/Assets/TounoWork/Scripts/`中。


## 重要程式碼位置
- AG避障程式碼主要位置在`/AG_final/GF/Assets/TounoWork/Scripts/`中，其他的程式碼或物件為unity專案所需。

    ![](https://i.imgur.com/UlTDqX9.png)

- 程式碼宏觀用途，==黃色網底==為避障、攻擊，以及主要策略的程式碼：
    - ==Controller：主要避障、陣型推估、打擊航母等function位置。==

    - DubinsGeneratePath：產生計算dubin curve以及其他資訊的程式碼，其中會呼叫DubinMath。
    - DubinMath：Dubin曲線底層數學方程式，會被DubinsGeneratePath呼叫。
    - GeneratePath：產生避障路徑，裡面會使用到GetNewTarget、DubinMath等function產生避障路徑。
    - GetNewTarget：推移新的目標圓，在產生避障路徑時會使用到。
    - Improved_APF：IAPF攻擊路徑規劃的程式。
    - MathFunction：一些底層的數學公式、公切線計算，避障過程的迴轉圓之間如何產生更多的迴轉圓的程式邏輯。
    - MultiSimulateWorker：Unity需要使用的程式碼。
    - OneDubinsPath：定義dubin curve的物件型態類別。
    - PageUI：Unity需要使用的程式碼。
    - PathGiver：Unity需要使用的程式碼，定義飛彈轉彎或直線時是否開啟RF。
    - PathGroupMaker：將避障演算法所計算出來的迴轉圓利用程式碼串接。
    - PathMaker：設定初始探索路徑。
    - PredictFormation：有航向資訊時的船團推估演算法。
    - RF_PathPainter：Unity需要使用的程式碼，繪製RF的偵搜扇形。
    - RF：看似沒屁用，模式模擬組設計時就存在了。
    - SettingControl：Unity需要使用的程式碼。
    - ShipNode：Unity需要使用的程式碼。
    - ShipSettingControl：Unity需要使用的程式碼，大量測試程式碼在此編寫，包含大量測試的船團初始方向與位置。
    - ==ShipWork：偵測到護衛艦或航母所要執行的策略皆設計於此，會串接到Controller執行對應的function。==
    - SystemSurroundingSetting：Unity需要使用的程式碼。
    - Timer：Unity需要使用的程式碼。
    - TurnCircle：迴轉圓類別的定義。
    - UtilsWithoutDirection：沒有航向時的陣型推估演算法。

## ShipWork中重要程式碼片段OnTriggerEnter
- 此段程式碼主要用於展示RF偵搜到船艦(護衛艦或航母)時，會觸發的一些策略，其中會呼叫Controller中的function來執行對應的動作。

### 偵搜到護衛艦

1. 呼叫 Controller 的 Reorganize_ships 以獲得當前所有已觀測的航母與護衛艦位置。

2. 透過所獲得的航母位置，呼叫 Controller 的 Hit_CV 來規劃打擊航母的方式，直線或IAPF。
![](https://i.imgur.com/BGdb81F.png)

### 偵搜到護衛艦
偵測到的船艦，距離航母小於28公里，表示為護衛艦

- 若已偵測到3~5艘護衛艦 & 偵測到船艦為新船艦 & 尚未偵測到航母 & 猜陣型功能開啟
    1. 呼叫 Controller 的 Predict_CV 來推估船團和獲得可能的航母位置。
    2. 呼叫 Controller 的 Hit_CV 來規劃打擊航母的方式，直線或IAPF。

![](https://i.imgur.com/loPR3Ur.png)

- 如果偵測到新船艦 & 尚未偵測到航母
    1. 呼叫 Controller 的 Reorganize_ships重新彙整所有觀測到的護衛艦。
    2. 呼叫 Controller 的 Hit_CV 來規劃打擊航母的方式，直線或IAPF。

![](https://i.imgur.com/vuhhNrZ.png)

- 如果尚未偵測到航母 & 尚未進行陣型推估預測航母 & 避障策略開啟
    1. 透過全域變數區分使用複雜避障策略(三角動態)或簡單避障
    2. 分別呼叫 Controller 的避障策略進行避障

![](https://i.imgur.com/hCSzDQ5.png)

