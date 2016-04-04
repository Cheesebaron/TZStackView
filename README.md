# TZStackView
This is a port of [Tom van Zummeren's][tvz] [TZStackView][tzstackview], which provides a backport of UIStackView, which was introduced in iOS 9.

StackView provides an easy way to lay out views horizontally and vertically and UIStackView is the new recommended way to lay out things in iOS 9,
instead of resolving to use explicit view constraints.

## Features
- Compatible with iOS 7 and 8
- Complete API of `UIStackView` with all distribution and alignment options
- Animation of hidden property

- Support for storyboards.

## Installing
Add the TZStackView nuget to your Xamarin.iOS project

> Install-Package TZStackView

## Usage
Given `view1`, `view2` and `view3` have intrinsic content sizes set to 100x100, 80x80 and 60x60 respectively.

```
var stackView = new StackView(new UIView[] {view1, view2, view3})
{
    Axis = UILayoutConstraintAxis.Vertical,
    Distribution = Distribution.FillEqually,
    Alignment = Alignment.Center,
    Spacing = 25
};
```

This will produce the following layout.
![Layout Example][layout]

To animate adding or removing a view from the arranged subviews simply toggle the `Hidden` property on the view.

```
UIView.AnimateNotify(0.6, 0, 0.7f, 0, UIViewAnimationOptions.AllowUserInteraction, 
	() => { view.Hidden = true; }, completed => { });
```

## Migrating to UIStackView
If you at some point want to make iOS 9 the minimum target of your application, you will want to replace this with `UIStackView`.
Since `TZStackView` is a drop in replacement of `UIStackView`, you should be able to just use `UIStackView` instead.

```
var stackView = StackView(subViews);
```

```
var stackView = UIStackView(subViews);
```

## License
TZStackView is licensed under the MIT License. See the [LICENSE](/LICENSE) file for details.

[tvz]: https://github.com/tomvanzummeren
[tzstackview]: https://github.com/tomvanzummeren/TZStackView
[layout]: /assets/layout-example.png
