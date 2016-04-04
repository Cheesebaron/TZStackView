using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Foundation;
using UIKit;
using CoreGraphics;
using CoreAnimation;

namespace TZStackView
{
    [Register("TZStackView")]
    [DesignTimeVisible(true)]
    public class StackView : UIView
    {
        private readonly ObservableCollection<UIView> _arrangedSubviews = new ObservableCollection<UIView>();
        private readonly List<UIView> _registeredHiddenObserverViews = new List<UIView>();
        private readonly List<UIView> _animatingToHiddenViews = new List<UIView>();
        private readonly List<UIView> _spacerViews = new List<UIView>();
        private readonly List<NSLayoutConstraint> _stackViewConstraints = new List<NSLayoutConstraint>();
        private readonly List<NSLayoutConstraint> _subviewConstraints = new List<NSLayoutConstraint>();
        private Alignment _alignment = Alignment.Fill;
        private Distribution _distribution = Distribution.Fill;
        private UILayoutConstraintAxis _axis = UILayoutConstraintAxis.Horizontal;

        [Export("Disribution"), Browsable(true)]
        public Distribution Distribution
        {
            get { return _distribution; }
            set
            {
                _distribution = value;
                SetNeedsUpdateConstraints();
            }
        }

        [Export("Alignment"), Browsable(true)]
        public Alignment Alignment
        {
            get { return _alignment; }
            set
            {
                _alignment = value;
                SetNeedsUpdateConstraints();
            }
        }

        [Export("Axis"), Browsable(true)]
        public UILayoutConstraintAxis Axis
        {
            get { return _axis; }
            set
            {
                _axis = value;
                SetNeedsUpdateConstraints();
            }
        }

        public IEnumerable<UIView> ArrangedSubviews => _arrangedSubviews;

        [Export("LayoutMarginsRelative"), Browsable(true)]
        public bool LayoutMarginsRelativeArrangement { get; set; } = false;

        [Export("Spacing"), Browsable(true)]
        public float Spacing { get; set; } = 0f;

        private void ArrangedSubviewsChanged(object s,
            NotifyCollectionChangedEventArgs args)
        {
            var oldItems = args.OldItems;
			if (oldItems != null)
            	foreach (var subview in oldItems.OfType<UIView>())
                	RemoveHiddenListener(subview);

            var newItems = args.NewItems;
			if (newItems != null)
            	foreach(var subview in newItems.OfType<UIView>())
                	AddHiddenListener(subview);
        }

        public StackView(IntPtr handle) : base(handle) { }

        public StackView(IEnumerable<UIView> arrangedSubviews = null) 
            : base(CGRect.Empty)
        {
			_arrangedSubviews.CollectionChanged += ArrangedSubviewsChanged;
            Initialize(arrangedSubviews);
        }

        public override void AwakeFromNib()
        {
            Initialize();
        }

        private void Initialize(IEnumerable<UIView> arrangedSubviews = null)
        {
            if (arrangedSubviews == null)
                arrangedSubviews = new List<UIView>();

            foreach (var subview in arrangedSubviews)
            {
                subview.TranslatesAutoresizingMaskIntoConstraints = false;
                AddSubview(subview);
                _arrangedSubviews.Add(subview);
            }
        }

        private void AddHiddenListener(UIView view)
        {
			view.Layer.AddObserver(this, "hidden", NSKeyValueObservingOptions.OldNew, IntPtr.Zero);
            _registeredHiddenObserverViews.Add(view);
        }

        private void RemoveHiddenListener(UIView view)
        {
            if (_registeredHiddenObserverViews.Contains(view))
            {
				view.Layer.RemoveObserver(this, (NSString)"hidden", IntPtr.Zero);
                _registeredHiddenObserverViews.Remove(view);
            }
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change,
            IntPtr context)
        {
			if (keyPath != "hidden") return;

            var layer = ofObject as CALayer;

            // Delegate/WeakDelegate is usually the UIView on CALayer
			var view = layer?.WeakDelegate as UIView;
			if (view == null)
				return;

            var hidden = view.Hidden;
            var previousValue = (change["old"] as NSNumber)?.BoolValue;
            if (previousValue.HasValue && 
                previousValue.Value == hidden)
            {
                return;
            }

            if (hidden)
            {
                _animatingToHiddenViews.Add(view);
            }

            SetNeedsUpdateConstraints();
            SetNeedsLayout();
            LayoutIfNeeded();

            RemoveHiddenListener(view);
            //view.Hidden = false;

			var hidingAnimation = layer.AnimationForKey ("bounds.size");

			Action animationFinished = () => {
				var strongLayer = layer;
				strongLayer.Hidden = hidden;
				var strongView = view;
				if (_animatingToHiddenViews.Contains(strongView)){
					_animatingToHiddenViews.Remove(strongView);
				}
				AddHiddenListener(strongView);
			};

			if (hidingAnimation != null)
			{
				var group = new CAAnimationGroup ();
				group.Animations = new CAAnimation[0];
				group.Delegate = new AnimationDelegate {
					AnimationStoppedCallback = animationFinished
				};

				layer.AddAnimation (group, "TZSV-hidden-callback");
			}
			else{
				animationFinished ();
			}
        }

        public void AddArrangedSubview(UIView view)
        {
            view.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(view);
            _arrangedSubviews.Add(view);
        }

        public void RemoveArrangedSubview(UIView view)
        {
            if (_arrangedSubviews.Contains(view))
                _arrangedSubviews.Remove(view);
        }

        public void InsertArrangedSubview(UIView view, int atIndex)
        {
            view.TranslatesAutoresizingMaskIntoConstraints = false;
            AddSubview(view);
            _arrangedSubviews.Insert(atIndex, view);
        }

        public override void WillRemoveSubview(UIView view)
        {
            RemoveArrangedSubview(view);
        }

        public override void UpdateConstraints()
        {
            RemoveConstraints(_stackViewConstraints.ToArray());
            _stackViewConstraints.Clear();

            foreach (var arrangedSubview in _arrangedSubviews)
                arrangedSubview.RemoveConstraints(_subviewConstraints.ToArray());
            _subviewConstraints.Clear();

            foreach (var arrangedSubview in _arrangedSubviews)
            {
                if (Alignment != Alignment.Fill)
                {
                    NSLayoutConstraint guideConstraint = null;
                    if (Axis == UILayoutConstraintAxis.Horizontal)
                    {
                        guideConstraint = Constraint(arrangedSubview, NSLayoutAttribute.Height,
                            view2: null, attr2: NSLayoutAttribute.NoAttribute, constant: 0, priority: 25);
                    }
                    else if (Axis == UILayoutConstraintAxis.Vertical)
                    {
                        guideConstraint = Constraint(arrangedSubview, NSLayoutAttribute.Width,
                            view2: null, attr2: NSLayoutAttribute.NoAttribute, constant: 0, priority: 25);
                    }

                    _subviewConstraints.Add(guideConstraint);
                    arrangedSubview.AddConstraint(guideConstraint);
                }

                if (IsHidden(arrangedSubview))
                {
                    NSLayoutConstraint hiddenConstraint = null;
                    if (Axis == UILayoutConstraintAxis.Horizontal)
                    {
                        hiddenConstraint = Constraint(arrangedSubview, NSLayoutAttribute.Width,
                            view2: null, attr2: NSLayoutAttribute.NoAttribute);
                    }
                    else if (Axis == UILayoutConstraintAxis.Vertical)
                    {
                        hiddenConstraint = Constraint(arrangedSubview, NSLayoutAttribute.Height,
                            view2: null, attr2: NSLayoutAttribute.NoAttribute);
                    }
                    _subviewConstraints.Add(hiddenConstraint);
                    arrangedSubview.AddConstraint(hiddenConstraint);
                }
            }

            foreach(var spacerView in _spacerViews)
                spacerView.RemoveFromSuperview();
            _spacerViews.Clear();

            if (_arrangedSubviews.Any())
            {
                var visibleArrangedSubviews = _arrangedSubviews.Where(v => !IsHidden(v)).ToArray();

                switch (Distribution)
                {
                    case Distribution.Fill:
                    case Distribution.FillEqualy:
                    case Distribution.FillProportionally: {
                        if (Alignment != Alignment.Fill || LayoutMarginsRelativeArrangement)
                            AddSpacerView();

                        _stackViewConstraints.AddRange(CreateMatchEdgesConstraints(_arrangedSubviews));
                        _stackViewConstraints.AddRange(CreateFirstAndLastViewMatchEdgesConstraints());

                        if (Alignment == Alignment.FirstBaseline &&
                            Axis == UILayoutConstraintAxis.Horizontal)
                            _stackViewConstraints.Add(Constraint(this, NSLayoutAttribute.Height,
                                attr2: NSLayoutAttribute.NoAttribute, priority: 49));

                        if (Distribution == Distribution.FillEqualy)
                            _stackViewConstraints.AddRange(CreateFillEquallyConstraints(_arrangedSubviews));
                        if (Distribution == Distribution.FillProportionally)
                            _stackViewConstraints.AddRange(
                                CreateFillProportionallyConstraints(_arrangedSubviews));

                        _stackViewConstraints.AddRange(CreateFillConstraints(_arrangedSubviews,
                            constant: Spacing));
                        break;
                    }
                    case Distribution.EqualSpacing: {
                        var views = new List<UIView>();
                        var index = 0;
                        foreach (var arrangedSubview in _arrangedSubviews)
                        {
                            if (IsHidden(arrangedSubview))
                                continue;
                            if (index > 0)
                                views.Add(AddSpacerView());
                            views.Add(arrangedSubview);
                            index++;
                        }
                        if (_spacerViews.Count == 0)
                            AddSpacerView();

                        _stackViewConstraints.AddRange(CreateMatchEdgesConstraints(_arrangedSubviews));
                        _stackViewConstraints.AddRange(CreateFirstAndLastViewMatchEdgesConstraints());

                        if (Axis == UILayoutConstraintAxis.Horizontal)
                        {
                            _stackViewConstraints.Add(Constraint(this, NSLayoutAttribute.Width,
                                attr2: NSLayoutAttribute.NoAttribute, priority: 49));
                            if (Alignment == Alignment.FirstBaseline)
                                _stackViewConstraints.Add(Constraint(this, NSLayoutAttribute.Height,
                                    attr2: NSLayoutAttribute.NoAttribute, priority: 49));
                        }
                        else if (Axis == UILayoutConstraintAxis.Vertical)
                            _stackViewConstraints.Add(Constraint(this, NSLayoutAttribute.Height, attr2: NSLayoutAttribute.NoAttribute, priority: 49));

                        _stackViewConstraints.AddRange(CreateFillConstraints(views));
                        _stackViewConstraints.AddRange(CreateFillEquallyConstraints(_spacerViews));
                        _stackViewConstraints.AddRange(CreateFillConstraints(_arrangedSubviews,
                            relatedBy: NSLayoutRelation.GreaterThanOrEqual, constant: Spacing));
                        break;
                    }
                    case Distribution.EqualCentering: {
                        for (var i = 0; i < visibleArrangedSubviews.Length; i++)
                            if (i > 0) AddSpacerView();

                        if (_spacerViews.Count == 0)
                            AddSpacerView();

                        _stackViewConstraints.AddRange(CreateMatchEdgesConstraints(_arrangedSubviews));
                        _stackViewConstraints.AddRange(CreateFirstAndLastViewMatchEdgesConstraints());

                        if (Axis == UILayoutConstraintAxis.Horizontal)
                        {
                            _stackViewConstraints.Add(Constraint(this, NSLayoutAttribute.Width,
                                attr2: NSLayoutAttribute.NoAttribute, priority: 49));
                            if (Alignment == Alignment.FirstBaseline)
                                _stackViewConstraints.Add(Constraint(this, NSLayoutAttribute.Height,
                                    attr2: NSLayoutAttribute.NoAttribute, priority: 49));
                        }
                        else if (Axis == UILayoutConstraintAxis.Vertical)
                            _stackViewConstraints.Add(Constraint(this, NSLayoutAttribute.Height,
                                attr2: NSLayoutAttribute.NoAttribute, priority: 49));

                        UIView previousArrangedSubview = null;
                        var index = 0;
                        foreach (var arrangedSubview in visibleArrangedSubviews)
                        {
                            if (previousArrangedSubview != null)
                            {
                                var spacerView = _spacerViews[index - 1];

                                if (Axis == UILayoutConstraintAxis.Horizontal)
                                {
                                    _stackViewConstraints.Add(Constraint(previousArrangedSubview,
                                        NSLayoutAttribute.CenterX, view2: spacerView,
                                        attr2: NSLayoutAttribute.Leading));
                                    _stackViewConstraints.Add(Constraint(arrangedSubview,
                                        NSLayoutAttribute.CenterX, view2: spacerView,
                                        attr2: NSLayoutAttribute.Trailing));
                                }
                                else if (Axis == UILayoutConstraintAxis.Vertical)
                                {
                                    _stackViewConstraints.Add(Constraint(previousArrangedSubview,
                                        NSLayoutAttribute.CenterY, view2: spacerView,
                                        attr2: NSLayoutAttribute.Top));
                                    _stackViewConstraints.Add(Constraint(arrangedSubview,
                                        NSLayoutAttribute.CenterY, view2: spacerView,
                                        attr2: NSLayoutAttribute.Bottom));
                                }
                            }

                            previousArrangedSubview = arrangedSubview;
                            index++;
                        }

                        _stackViewConstraints.AddRange(CreateFillEquallyConstraints(_spacerViews,
                            priority: 150));
                        _stackViewConstraints.AddRange(CreateFillConstraints(_arrangedSubviews,
                            relatedBy: NSLayoutRelation.GreaterThanOrEqual, constant: Spacing));
                        break;
                    }
                }

                if (_spacerViews.Any())
                    _stackViewConstraints.AddRange(CreateSurroundingSpacerViewConstraints(
                        _spacerViews[0], views: visibleArrangedSubviews));

                if (LayoutMarginsRelativeArrangement && _spacerViews.Any())
                {
                    var first = _spacerViews[0];
                    _stackViewConstraints.Add(Constraint(this, NSLayoutAttribute.BottomMargin, view2: first));
                    _stackViewConstraints.Add(Constraint(this, NSLayoutAttribute.LeftMargin, view2: first));
                    _stackViewConstraints.Add(Constraint(this, NSLayoutAttribute.TopMargin, view2: first));
                    _stackViewConstraints.Add(Constraint(this, NSLayoutAttribute.RightMargin, view2: first));
                }
                AddConstraints(_stackViewConstraints.ToArray());
            }

            base.UpdateConstraints();
        }

        private SpacerView AddSpacerView()
        {
            var spacerView = new SpacerView {TranslatesAutoresizingMaskIntoConstraints = false};

            _spacerViews.Add(spacerView);
            InsertSubview(spacerView, 0);

            return spacerView;
        }

        private IEnumerable<NSLayoutConstraint> CreateSurroundingSpacerViewConstraints(UIView spacerView,
            IEnumerable<UIView> views)
        {
            if (Alignment == Alignment.Fill)
				return new NSLayoutConstraint[0];

            var topPriority = 1000f;
            var topRelation = NSLayoutRelation.LessThanOrEqual;

            var bottomPriority = 1000f;
            var bottomRelation = NSLayoutRelation.GreaterThanOrEqual;

            if (Alignment == Alignment.Top || Alignment == Alignment.Leading)
            {
                topPriority = 999.5f;
                topRelation = NSLayoutRelation.Equal;
            }

            if (Alignment == Alignment.Bottom || Alignment == Alignment.Trailing)
            {
                bottomPriority = 999.5f;
                bottomRelation = NSLayoutRelation.Equal;
            }

            var constraints = new List<NSLayoutConstraint>();
            foreach (var view in views)
            {
                if (Axis == UILayoutConstraintAxis.Horizontal)
                {
                    constraints.Add(Constraint(spacerView, NSLayoutAttribute.Top, topRelation, view,
                        priority: topPriority));
                    constraints.Add(Constraint(spacerView, NSLayoutAttribute.Bottom, bottomRelation, view,
                        priority: bottomPriority));
                }
                else if (Axis == UILayoutConstraintAxis.Vertical)
                {
                    constraints.Add(Constraint(spacerView, NSLayoutAttribute.Leading, topRelation, view,
                        priority: topPriority));
                    constraints.Add(Constraint(spacerView, NSLayoutAttribute.Trailing, bottomRelation, view,
                        priority: bottomPriority));
                }
            }

            if (Axis == UILayoutConstraintAxis.Horizontal)
                constraints.Add(Constraint(spacerView, NSLayoutAttribute.Height,
                    attr2: NSLayoutAttribute.NoAttribute, constant: 0, priority: 51));
            else if (Axis == UILayoutConstraintAxis.Vertical)
                constraints.Add(Constraint(spacerView, NSLayoutAttribute.Width,
                    attr2: NSLayoutAttribute.NoAttribute, constant: 0, priority: 51));

            return constraints;
        }

        private IEnumerable<NSLayoutConstraint> CreateFillProportionallyConstraints(
            IEnumerable<UIView> views)
        {
            var viewss = views.ToArray();
            var constraints = new List<NSLayoutConstraint>();

            nfloat totalSize = 0f;
            var totalCount = 0;

            foreach (var subview in viewss)
            {
                if (IsHidden(subview)) continue;

                if (Axis == UILayoutConstraintAxis.Horizontal)
                    totalSize += subview.IntrinsicContentSize.Width;
                else if (Axis == UILayoutConstraintAxis.Vertical)
                    totalSize += subview.IntrinsicContentSize.Height;
                totalCount++;
            }

            totalSize += (totalCount - 1)*Spacing;

            var priority = 1000f;
            var countDownPriority = viewss.Count(v => !IsHidden(v)) > 1;

            foreach (var subview in viewss)
            {
                if (countDownPriority)
                    priority--;

                if (IsHidden(subview)) continue;

                if (Axis == UILayoutConstraintAxis.Horizontal)
                {
                    var multiplier = subview.IntrinsicContentSize.Width/totalSize;
                    constraints.Add(Constraint(subview, NSLayoutAttribute.Width,
                        multiplier: (float) multiplier, priority: priority));
                }
                else if (Axis == UILayoutConstraintAxis.Vertical)
                {
                    var multiplier = subview.IntrinsicContentSize.Height / totalSize;
                    constraints.Add(Constraint(subview, NSLayoutAttribute.Height,
                        multiplier: (float)multiplier, priority: priority));
                }
            }

            return constraints;
        }

        private IEnumerable<NSLayoutConstraint> CreateFillEquallyConstraints(
            IEnumerable<UIView> views, float priority = 1000)
        {
            if (Axis == UILayoutConstraintAxis.Horizontal)
                return EqualAttributes(views.Where(v => !IsHidden(v)), attribute: NSLayoutAttribute.Width, priority: priority);
            return EqualAttributes(views.Where(v => !IsHidden(v)), attribute: NSLayoutAttribute.Height, priority: priority);
        }

        private IEnumerable<NSLayoutConstraint> CreateFillConstraints(
            IEnumerable<UIView> views, float priority = 1000, NSLayoutRelation relatedBy = NSLayoutRelation.Equal, float constant = 0)
        {
            var constraints = new List<NSLayoutConstraint>();

            var viewss = views.ToArray();

            UIView previousView = null;
            foreach(var view in viewss)
            {
                if (previousView != null)
                {
                    var c = 0f;
                    if (!IsHidden(previousView) && !IsHidden(view))
                        c = constant;
                    else if (IsHidden(previousView) && !IsHidden(view) && !Equals(viewss.FirstOrDefault(), previousView))
                        c = (constant / 2f);
                    else if (!IsHidden(previousView) && IsHidden(view) && !Equals(viewss.LastOrDefault(), view))
                        c = (constant / 2f);

                    if (Axis == UILayoutConstraintAxis.Horizontal)
                        constraints.Add(Constraint(view, NSLayoutAttribute.Leading, relatedBy, previousView, NSLayoutAttribute.Trailing, constant: c, priority: priority));
                    else if (Axis == UILayoutConstraintAxis.Vertical)
                        constraints.Add(Constraint(view, NSLayoutAttribute.Top, relatedBy, previousView, 
                            NSLayoutAttribute.Bottom, constant: c, priority: priority));
                }
                previousView = view;
            }

            return constraints;
        }

        private IEnumerable<NSLayoutConstraint> CreateMatchEdgesConstraints(
            IEnumerable<UIView> views)
        {
            var constraints = new List<NSLayoutConstraint>();

            if (Axis == UILayoutConstraintAxis.Horizontal)
            {
                if (Alignment == Alignment.Fill)
                {
                    constraints.AddRange(EqualAttributes(views, NSLayoutAttribute.Bottom));
                    constraints.AddRange(EqualAttributes(views, NSLayoutAttribute.Top));
                }
                else if (Alignment == Alignment.Center)
                    constraints.AddRange(EqualAttributes(views, NSLayoutAttribute.CenterY));
                else if (Alignment == Alignment.Leading || Alignment == Alignment.Top)
                    constraints.AddRange(EqualAttributes(views, NSLayoutAttribute.Top));
                else if (Alignment == Alignment.Trailing || Alignment == Alignment.Bottom)
                    constraints.AddRange(EqualAttributes(views, NSLayoutAttribute.Bottom));
                else if (Alignment == Alignment.FirstBaseline)
                    constraints.AddRange(EqualAttributes(views, NSLayoutAttribute.FirstBaseline));
            }
            else if (Axis == UILayoutConstraintAxis.Vertical)
            {
                if (Alignment == Alignment.Fill)
                {
                    constraints.AddRange(EqualAttributes(views, NSLayoutAttribute.Leading));
                    constraints.AddRange(EqualAttributes(views, NSLayoutAttribute.Trailing));
                }
                else if (Alignment == Alignment.Center)
                    constraints.AddRange(EqualAttributes(views, NSLayoutAttribute.CenterX));
                else if (Alignment == Alignment.Leading || Alignment == Alignment.Top)
                    constraints.AddRange(EqualAttributes(views, NSLayoutAttribute.Leading));
                else if (Alignment == Alignment.Trailing || Alignment == Alignment.Bottom)
                    constraints.AddRange(EqualAttributes(views, NSLayoutAttribute.Trailing));
            }

            return constraints;
        }

        private IEnumerable<NSLayoutConstraint> CreateFirstAndLastViewMatchEdgesConstraints()
        {
            var constraints = new List<NSLayoutConstraint>();

            var visibleViews = _arrangedSubviews.Where(v => !IsHidden(v)).ToArray();
            var firstView = visibleViews.FirstOrDefault();
            var lastView = visibleViews.LastOrDefault();

            var topView = _arrangedSubviews.FirstOrDefault();
            var bottomView = topView;

            
            if (_spacerViews.Any())
            {
				var spacerView = _spacerViews[0];
                if (Alignment == Alignment.Center)
                {
                    topView = spacerView;
                    bottomView = spacerView;
                }
                else if (Alignment == Alignment.Top || Alignment == Alignment.Leading)
                    bottomView = spacerView;
                else if (Alignment == Alignment.Bottom || Alignment == Alignment.Trailing)
                    topView = spacerView;
                else if (Alignment == Alignment.FirstBaseline)
                {
                    if (Axis == UILayoutConstraintAxis.Horizontal)
                        bottomView = spacerView;
                    else if (Axis == UILayoutConstraintAxis.Vertical)
                    {
                        topView = spacerView;
                        bottomView = spacerView;
                    }
                }
            }

			var firstItem = LayoutMarginsRelativeArrangement ? _spacerViews.FirstOrDefault() : this;

            if (Axis == UILayoutConstraintAxis.Horizontal)
            {
                if (firstView != null)
                    constraints.Add(Constraint(firstItem, NSLayoutAttribute.Leading, view2: firstView));

                if (lastView != null)
                    constraints.Add(Constraint(firstItem, NSLayoutAttribute.Trailing, view2: lastView));

                constraints.Add(Constraint(firstItem, NSLayoutAttribute.Top, view2: topView));
                constraints.Add(Constraint(firstItem, NSLayoutAttribute.Bottom, view2: bottomView));

                if (Alignment == Alignment.Center)
                    constraints.Add(Constraint(firstItem, NSLayoutAttribute.CenterY, view2: _arrangedSubviews.First()));
            }
            else if (Axis == UILayoutConstraintAxis.Vertical)
            {
                if (firstView != null)
                    constraints.Add(Constraint(firstItem, NSLayoutAttribute.Top, view2: firstView));

                if (lastView != null)
                    constraints.Add(Constraint(firstItem, NSLayoutAttribute.Bottom, view2: lastView));

                constraints.Add(Constraint(firstItem, NSLayoutAttribute.Leading, view2: topView));
                constraints.Add(Constraint(firstItem, NSLayoutAttribute.Trailing, view2: bottomView));

                if (Alignment == Alignment.Center)
                    constraints.Add(Constraint(firstItem, NSLayoutAttribute.CenterX, view2: _arrangedSubviews.First()));
            }

            return constraints;
        }

        private IEnumerable<NSLayoutConstraint> EqualAttributes(IEnumerable<UIView> views, NSLayoutAttribute attribute, 
            float priority = 1000)
        {
            var currentPriority = priority;
            var constraints = new List<NSLayoutConstraint>();

            var viewss = views.ToArray();

            if (views != null && viewss.Any())
            {
                UIView firstView = null;
                var countDownPriority = currentPriority < 1000;
                foreach(var view in viewss)
                {
                    if (firstView != null)
                        constraints.Add(Constraint(firstView, attribute, view2: view, priority: currentPriority));
                    else
                        firstView = view;

                    if (countDownPriority)
                        currentPriority--;
                }
            }

            return constraints;
        }

        private NSLayoutConstraint Constraint(NSObject view1, NSLayoutAttribute attr1,
            NSLayoutRelation relation = NSLayoutRelation.Equal, NSObject view2 = null,
            NSLayoutAttribute? attr2 = null, float multiplier = 1, float constant = 0,
            float priority = 1000)
        {
            var attribute2 = attr2 ?? attr1;

            var constraint = NSLayoutConstraint.Create(view1, attr1, relation, view2, attribute2,
                multiplier, constant);
            constraint.Priority = priority;
            return constraint;
        }

        private bool IsHidden(UIView view)
        {
            if (view.Hidden)
                return true;
            return _animatingToHiddenViews.IndexOf(view) >= 0;
        }
    }
}
