using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using TZStackView;
using UIKit;

namespace Sample
{
    public class ViewController : UIViewController
    {
        private StackView _stackView;

        private UIButton _resetButton = new UIButton(UIButtonType.System);
        private UILabel _instructionLabel = new UILabel();

        private UISegmentedControl _axisSegmentedControl;
        private UISegmentedControl _alignmentSegmentedControl;
        private UISegmentedControl _distributionSegmentedControl;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            EdgesForExtendedLayout = UIRectEdge.None;

            View.BackgroundColor = UIColor.Black;
            Title = "TZStackView";

            _stackView = new StackView(CreateViews())
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Axis = UILayoutConstraintAxis.Vertical,
                Distribution = Distribution.Fill,
                Alignment = Alignment.Fill,
                Spacing = 15
            };
            Add(_stackView);

            _instructionLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            _instructionLabel.Font = UIFont.SystemFontOfSize(15);
            _instructionLabel.Text = "Tap any of the boxes to set Hidden=true";
            _instructionLabel.TextColor = UIColor.White;
            _instructionLabel.Lines = 0;
            _instructionLabel.SetContentCompressionResistancePriority(900, UILayoutConstraintAxis.Horizontal);
            _instructionLabel.SetContentHuggingPriority(1000, UILayoutConstraintAxis.Vertical);
            Add(_instructionLabel);

            _resetButton.TranslatesAutoresizingMaskIntoConstraints = false;
            _resetButton.SetTitle("Reset", UIControlState.Normal);
            _resetButton.TouchUpInside += (_, __) => Reset();
            _resetButton.SetContentCompressionResistancePriority(1000, UILayoutConstraintAxis.Horizontal);
            _resetButton.SetContentHuggingPriority(1000, UILayoutConstraintAxis.Horizontal);
            _resetButton.SetContentHuggingPriority(1000, UILayoutConstraintAxis.Vertical);
            Add(_resetButton);

			_axisSegmentedControl = new UISegmentedControl ();
			_axisSegmentedControl.InsertSegment ("Vertical", 0, false);
			_axisSegmentedControl.InsertSegment ("Horizontal", 1, false);
            _axisSegmentedControl.SelectedSegment = 0;
            _axisSegmentedControl.ValueChanged += (s, __) => AxisChanged(s as UISegmentedControl);
            _axisSegmentedControl.SetContentCompressionResistancePriority(1000, UILayoutConstraintAxis.Horizontal);
            _axisSegmentedControl.TintColor = UIColor.LightGray;

			_alignmentSegmentedControl = new UISegmentedControl ();
			_alignmentSegmentedControl.InsertSegment ("Fill", 0, false);
			_alignmentSegmentedControl.InsertSegment ("Center", 1, false);
			_alignmentSegmentedControl.InsertSegment ("Leading", 2, false);
			_alignmentSegmentedControl.InsertSegment ("Top", 3, false);
			_alignmentSegmentedControl.InsertSegment ("Trailing", 4, false);
			_alignmentSegmentedControl.InsertSegment ("Bottom", 5, false);
			_alignmentSegmentedControl.InsertSegment ("FirstBaseline", 6, false);
            _alignmentSegmentedControl.SelectedSegment = 0;
            _alignmentSegmentedControl.ValueChanged += (s, __) => AlignmentChanged(s as UISegmentedControl);
            _alignmentSegmentedControl.SetContentCompressionResistancePriority(1000, UILayoutConstraintAxis.Horizontal);
            _alignmentSegmentedControl.TintColor = UIColor.LightGray;

			_distributionSegmentedControl = new UISegmentedControl ();
			_distributionSegmentedControl.InsertSegment ("Fill", 0, false);
			_distributionSegmentedControl.InsertSegment ("FillEqually", 1, false);
			_distributionSegmentedControl.InsertSegment ("FillProportionally", 2, false);
			_distributionSegmentedControl.InsertSegment ("EqualSpacing", 3, false);
			_distributionSegmentedControl.InsertSegment ("EqualCentering", 4, false);
            _distributionSegmentedControl.SelectedSegment = 0;
            _distributionSegmentedControl.ValueChanged += (s, __) => DistributionChanged(s as UISegmentedControl);
            _distributionSegmentedControl.TintColor = UIColor.LightGray;

            var controlsLayoutContainer = new StackView(new UIView[]
            {
                _axisSegmentedControl, _alignmentSegmentedControl, _distributionSegmentedControl
            })
            {
                Axis = UILayoutConstraintAxis.Vertical,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Spacing = 5
            };
            controlsLayoutContainer.SetContentHuggingPriority(1000, UILayoutConstraintAxis.Vertical);
            Add(controlsLayoutContainer);

            var views = new NSMutableDictionary<NSString, NSObject>
            {
                {new NSString("instructionLabel"), _instructionLabel },
                {new NSString("resetButton"), _resetButton },
                {new NSString("stackView"), _stackView },
                {new NSString("controlsLayoutContainer"), controlsLayoutContainer }
            };

            var metrics = new NSMutableDictionary<NSString, NSObject>
            {
                {new NSString("gap"), FromObject(10) },
                {new NSString("topspacing"), FromObject(25) }
            };

			View.AddConstraints (NSLayoutConstraint.FromVisualFormat ("H:|-gap-[instructionLabel]-[resetButton]-gap-|", 
				NSLayoutFormatOptions.DirectionLeadingToTrailing, metrics, views));
			View.AddConstraints (NSLayoutConstraint.FromVisualFormat ("H:|[stackView]|", 
				NSLayoutFormatOptions.DirectionLeadingToTrailing, metrics, views));
			View.AddConstraints (NSLayoutConstraint.FromVisualFormat ("H:|[controlsLayoutContainer]|", 
				NSLayoutFormatOptions.DirectionLeadingToTrailing, metrics, views));

			View.AddConstraints (NSLayoutConstraint.FromVisualFormat (
				"V:|-topspacing-[instructionLabel]-gap-[controlsLayoutContainer]-gap-[stackView]|", 
				NSLayoutFormatOptions.DirectionLeadingToTrailing, metrics, views));
			View.AddConstraints (NSLayoutConstraint.FromVisualFormat (
				"V:|-topspacing-[resetButton]-gap-[controlsLayoutContainer]", 
				NSLayoutFormatOptions.DirectionLeadingToTrailing, metrics, views));

			View.SetNeedsLayout ();
        }

        private IEnumerable<UIView> CreateViews()
        {
            var redView = new ExplicitIntrinsicContentSizeView(new CGSize(100, 100), "Red");
            var greenView = new ExplicitIntrinsicContentSizeView(new CGSize(80, 80), "Green");
            var blueView = new ExplicitIntrinsicContentSizeView(new CGSize(60, 60), "Blue");
            var purpleView = new ExplicitIntrinsicContentSizeView(new CGSize(80, 80), "Purple");
            var yellowView = new ExplicitIntrinsicContentSizeView(new CGSize(100, 100), "Yellow");

            redView.BackgroundColor = UIColor.Red.ColorWithAlpha(0.75f);
            greenView.BackgroundColor = UIColor.Green.ColorWithAlpha(0.75f);
            blueView.BackgroundColor = UIColor.Blue.ColorWithAlpha(0.75f);
            purpleView.BackgroundColor = UIColor.Purple.ColorWithAlpha(0.75f);
            yellowView.BackgroundColor = UIColor.Yellow.ColorWithAlpha(0.75f);

            return new[] { redView, greenView, blueView, purpleView, yellowView };
        }

        private void Reset()
        {
            UIView.AnimateNotify(0.6, 0, 0.7f, 0, UIViewAnimationOptions.AllowUserInteraction,
                () => {
                    foreach (var view in _stackView.ArrangedSubviews)
                        view.Hidden = false;
                },
                completed => { });
        }

        private void AxisChanged(UISegmentedControl sender)
        {
            if (sender == null) return;

            _stackView.Axis = sender.SelectedSegment == 0 ? 
                UILayoutConstraintAxis.Vertical : UILayoutConstraintAxis.Horizontal;
        }

        private void AlignmentChanged(UISegmentedControl sender)
        {
            if (sender == null) return;
            var index = sender.SelectedSegment;

            switch(index) {
                case 0:
                    _stackView.Alignment = Alignment.Fill;
                    break;
                case 1:
                    _stackView.Alignment = Alignment.Center;
                    break;
                case 2:
                    _stackView.Alignment = Alignment.Leading;
                    break;
                case 3:
                    _stackView.Alignment = Alignment.Top;
                    break;
                case 4:
                    _stackView.Alignment = Alignment.Trailing;
                    break;
                case 5:
                    _stackView.Alignment = Alignment.Bottom;
                    break;
                default:
                    _stackView.Alignment = Alignment.FirstBaseline;
                    break;
            }
        }

        private void DistributionChanged(UISegmentedControl sender)
        {
            if (sender == null) return;
            var index = sender.SelectedSegment;

            switch (index) {
                case 0:
                    _stackView.Distribution = Distribution.Fill;
                    break;
                case 1:
                    _stackView.Distribution = Distribution.FillEqualy;
                    break;
                case 2:
                    _stackView.Distribution = Distribution.FillProportionally;
                    break;
                case 3:
                    _stackView.Distribution = Distribution.EqualSpacing;
                    break;
                default:
                    _stackView.Distribution = Distribution.EqualCentering;
                    break;
            }
        }

        public override UIStatusBarStyle PreferredStatusBarStyle() {
            return UIStatusBarStyle.LightContent;
        }
    }

    public class ExplicitIntrinsicContentSizeView : UIView
    {
        private string _name;
        private CGSize _contentSize;

        public ExplicitIntrinsicContentSizeView(CGSize intrinsicContentSize, string name) : base(CGRect.Empty)
        {
            _name = name;
            _contentSize = intrinsicContentSize;

            var gestureRecognizer = new UITapGestureRecognizer(Tap);
            AddGestureRecognizer(gestureRecognizer);
            UserInteractionEnabled = true;
        }

        private void Tap()
        {
            UIView.AnimateNotify(0.6, 0, 0.7f, 0, UIViewAnimationOptions.AllowUserInteraction, 
                () => { 
					Hidden = true;
				}, 
                completed => { });
        }

        public override CGSize IntrinsicContentSize
        {
            get { return _contentSize; }
        }

        public override string Description
        {
            get { return _name; }
        }
    }
}
