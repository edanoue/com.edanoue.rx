# com.edanoue.rx

## Description

## Installation

- Add NuGet scoped registry in your project. See: [UnityNuGet](https://github.com/xoofx/UnityNuGet)
- Add this repository as git package in UPM.

### Note: Additional packages

- If you want to use TimeProvider dependent operators, you need to install `org.nuget.microsoft.bcl.timeprovider`
  package.

## Operators

### CombineLatest

```csharp
var s1 = new Subject<int>();
var s2 = new Subject<string>();

s1.
    .CombineLatest(s2, (x, y) => $"{x} {y}")
    .Subscribe(x => Debug.Log(x));

s1.OnNext(1); // >
s1.OnNext(2); // >
s2.OnNext(3); // > "2 3"
s1.OnNext(4); // > "4 3"
s2.OnNext(5); // > "4 5"
s2.OnNext(6); // > "4 6"

s1.OnCompleted();
s1.OnNext(7); // >
s2.OnNext(8); // > "4 8"
```

- 登録した Source すべての OnNext が呼ばれるまで遮断する
- 以降は各 Source 最新の値を指定した関数に入れて次に流す

> [!NOTE]
> Factory 版との違いは任意の型を組み合わせ可能なのと, 次のストリームに流す合成用の関数 (Select のような) を渡せる点

### Distinct

```csharp
var r = new List<int>();
var s = new Subject<int>();
s.Distinct().Subscribe(r.Add);

s.OnNext(1); // [1]
s.OnNext(2); // [1, 2]
s.OnNext(1); // [1, 2]
s.OnNext(2); // [1, 2]
s.OnNext(3); // [1, 2, 3]
```

- 一度登場した値は以降弾く

### DistinctBy

```csharp
var r = new List<(string, int)>();
var s = new Subject<(string, int)>();
s.DistinctBy(static x => x.Item1).Subscribe(r.Add);

s.OnNext(("foo", 1)); // [("foo", 1)]
s.OnNext(("bar", 2)); // [("foo", 1), ("bar", 2)]
s.OnNext(("foo", 3)); // [("foo", 1), ("bar", 2)]
s.OnNext(("bar", 4)); // [("foo", 1), ("bar", 2)]
s.OnNext(("baz", 5)); // [("foo", 1), ("bar", 2), ("baz", 5)]
```

- Distinct の判定部分を任意の関数に変更できる

### DistinctUntilChanged

```csharp
var r = new List<int>();
var s = new Subject<int>();
s.DistinctUntilChanged().Subscribe(r.Add);

s.OnNext(1); // [1]
s.OnNext(2); // [1, 2]
s.OnNext(2); // [1, 2]
s.OnNext(1); // [1, 2, 1]
s.OnNext(2); // [1, 2, 1, 2]
```

- 連続して登場した値は弾く

### DistinctUntilChangedBy

```csharp
var r = new List<(string, int)>();
var s = new Subject<(string, int)>();
s.DistinctUntilChangedBy(static x => x.Item1).Subscribe(r.Add);

s.OnNext(("foo", 1)); // [("foo", 1)]
s.OnNext(("bar", 2)); // [("foo", 1), ("bar", 2)]
s.OnNext(("bar", 3)); // [("foo", 1), ("bar", 2)]
s.OnNext(("foo", 4)); // [("foo", 1), ("bar", 2), ("foo", 4)]
s.OnNext(("bar", 5)); // [("foo", 1), ("bar", 2), ("foo", 4), ("bar", 5)]
```

- DistinctUntilChanged の判定部分を任意の関数に変更できる

### Select

```csharp
var r = new List<int>();
var s = new Subject<int>();
s.Select(x => x * 2).Subscribe(r.Add);

s.OnNext(1); // [2]
s.OnNext(2); // [2, 4]
```
```csharp
var r = new List<string>();
var s = new Subject<int>();
s.Select(x => $"{x}").Subscribe(r.Add);

s.OnNext(1); // ["1"]
s.OnNext(2); // ["1", "2"]
```

- Source を指定した関数で加工して次に流す

### Skip

```csharp
var s = new Subject<int>();
s.Skip(2).Subscribe(x => Debug.Log(x));

s.OnNext(1); // >
s.OnNext(2); // >
s.OnNext(3); // > 3
```

- 指定した回数に達するまで Source を遮断する

### SkipWhile

```csharp
var s = new Subject<int>();
s.SkipWhile(x => x <= 2).Subscribe(x => Debug.Log(x));

s.OnNext(1); // >
s.OnNext(2); // >
s.OnNext(3); // > 3
s.OnNext(2); // > 2
```

- 指定した条件を満たしている間 Source を遮断する
- 一度でも条件を満たさなかった場合それ以降は通す

### Take

```csharp
var isCompleted = false;
var s = new Subject<int>();
s.Take(2).Subscribe(
    x => Debug.Log(x),
    r => isCompleted = true
);

s.OnNext(1); // > 1 / isCompleted: false
s.OnNext(2); // > 2 / isCompleted: true
s.OnNext(3); // >   / isCompleted: true
```

- 指定した回数に達すると OnCompleted を呼ぶ

### TakeWhile

```csharp
var isCompleted = false;
var s = new Subject<int>();
s.TakeWhile(x => x <= 2).Subscribe(
    x => Debug.Log(x),
    r => isCompleted = true
);

s.OnNext(1); // > 1 / isCompleted: false
s.OnNext(2); // > 2 / isCompleted: false
s.OnNext(3); // >   / isCompleted: true
```

- 指定した条件を満たさなかった場合 OnCompleted を呼ぶ
- Take と異なり OnNext も呼ばない

### Where

```csharp
var s = new Subject<int>();
s.Where(x => x % 2 == 0).Subscribe(x => Debug.Log(x));

s.OnNext(1); // >
s.OnNext(2); // > 2
s.OnNext(3); // >
s.OnNext(4); // > 4
```

- 指定した条件を満たした場合のみ OnNext を通す

## Factory operators

### CombineLatest

```csharp
var s1 = new Subject<int>();
var s2 = new Subject<int>();

Observable
    .CombineLatest(s1, s2)
    .Subscribe(x => Debug.Log(x));

s1.OnNext(1); // >
s1.OnNext(2); // >
s2.OnNext(3); // > [2, 3]
s1.OnNext(4); // > [4, 3]
s2.OnNext(5); // > [4, 5]
s2.OnNext(6); // > [4, 6]

s1.OnCompleted();
s1.OnNext(7); // >
s2.OnNext(8); // > [4, 8]
```

> [!NOTE]
> Factory 版は同じ Type の Observable しか入らず, Array が帰ってくる

### Merge

```csharp
var s1 = new Subject<string>();
var s2 = new Subject<string>();
var isCompleted = false;

Observable
    .Merge(s1, s2)
    .Subscribe(
        x => Debug.Log(x),      // OnNext
        r => isCompleted = true // OnCompleted
    );

s1.OnNext("foo"); // > foo
s2.OnNext("bar"); // > bar

s1.Dispose(); // isCompleted: false
s2.Dispose(); // isCompleted: true
```

## TimeProvider dependent operators

> [!NOTE]
> 現在は Unity 内の Time ではなく, System の Time を使用している

### Debounce

```csharp
var result = 0;
var s = new Subject<int>();

// 通知が来るたびに2秒のタイマーをリセットする, 2秒立ったら最新の値をセットする
s.Debounce(TimeSpan.FromSeconds(2f)).Subscribe(x => result = x);

// 00:00:00
s.OnNext(1);
s.OnNext(2);
Debug.Log(result) // >

// 00:01:00
s.OnNext(3);
Debug.Log(result) // >

// 00:02:00
Debug.Log(result) // >

// 00:03:00
Debug.Log(result) // > 3
```

### Skip

```csharp
var isCompleted = false;
var s = new Subject<int>();
s.Skip(TimeSpan.FromSeconds(1f)).Subscribe(
    x => Debug.Log(x)
);

// 00:00:00
s.OnNext(1); // >

// 00:01:00
s.OnNext(2); // > 2
```

### Take

```csharp
var isCompleted = false;
var s = new Subject<int>();
s.Take(TimeSpan.FromSeconds(1f)).Subscribe(
    x => Debug.Log(x)
);

// 00:00:00
s.OnNext(1); // > 1

// 00:01:00
s.OnNext(2); // > 
```
