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

> [!NOTE]
> Factory 版との違いは任意の型を組み合わせ可能なのと, 次のストリームに流す合成用の関数 (Select のような) を渡せる点

### Select

```csharp
var s = new Subject<int>();
s.Select(x => x * 2).Subscribe(x => Debug.Log(x));

s.OnNext(1); // > 2
s.OnNext(2); // > 4
```

### Skip

```csharp
var s = new Subject<int>();
s.Skip(2).Subscribe(x => Debug.Log(x));

s.OnNext(1); // >
s.OnNext(2); // >
s.OnNext(3); // > 3
```

### SkipWhile

```csharp
var s = new Subject<int>();
s.SkipWhile(x => x <= 2).Subscribe(x => Debug.Log(x));

s.OnNext(1); // >
s.OnNext(2); // >
s.OnNext(3); // > 3

// 一度条件を満たすとそれ以降は通す
s.OnNext(2); // > 2
```

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

### Where

```csharp
var s = new Subject<int>();
s.Where(x => x % 2 == 0).Subscribe(x => Debug.Log(x));

s.OnNext(1); // >
s.OnNext(2); // > 2
s.OnNext(3); // >
s.OnNext(4); // > 4
```

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
