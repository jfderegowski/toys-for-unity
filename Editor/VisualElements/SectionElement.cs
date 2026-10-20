using System.Collections.Generic;
using fefek5.Toys.Editor.Icons;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.VisualElements
{
    /// <summary>
    /// A group of fields under a title with an optional icon, on a background a shade darker than the inspector.
    /// <see cref="VisualElement.Add"/> puts the fields under the title. Clicking the title collapses and expands it,
    /// animated; it starts collapsed.
    /// </summary>
    public class SectionElement : VisualElement
    {
        private const float ICON_SIZE = 16;
        private const float ARROW_SIZE = 12;
        private const float CORNER_RADIUS = 4;
        private const float HEADER_SPACING = 5;
        private const int ANIMATION_MS = 150;
        private const string ARROW_ICON_NAME = "DropdownArrow";

        private readonly string _saveKey;
        private readonly Image _arrow;
        private readonly VisualElement _clip;
        private readonly VisualElement _content;

        private bool _expanded;

        public Image Icon { get; }

        public Label Title { get; }

        public override VisualElement contentContainer => _content;

        /// <summary>
        /// Whether the fields are shown. With a save key it is kept in the <see cref="EditorPrefs"/>.
        /// </summary>
        public bool Expanded
        {
            get => _expanded;
            set
            {
                _expanded = value;

                _arrow.style.rotate = new Rotate(new Angle(value ? 0 : -90, AngleUnit.Degree));

                // Not laid out yet: it starts closed and, when expanded, opens at once to the height of the content,
                // through the GeometryChangedEvent of the content. Changes after that are animated.
                if (panel == null)
                {
                    _clip.style.height = 0;
                }
                else
                {
                    _clip.style.transitionDuration = new List<TimeValue> { new(ANIMATION_MS, TimeUnit.Millisecond) };
                    _clip.style.height = value ? _content.layout.height : 0;
                }

                if (_saveKey != null)
                    EditorPrefs.SetBool(_saveKey, value);
            }
        }

        /// <param name="title">Text of the header.</param>
        /// <param name="icon">Icon before the title; none when null.</param>
        /// <param name="saveKey">
        /// <see cref="EditorPrefs"/> key the collapsed state is kept under, e.g. the type and the title, so it stays
        /// when the inspector is rebuilt; null keeps it only while this element lives.
        /// </param>
        public SectionElement(string title, Texture icon = null, string saveKey = null)
        {
            _saveKey = saveKey;

            style.marginTop = 4;
            style.marginBottom = 4;
            style.paddingTop = 5;
            style.paddingRight = 5;
            style.paddingBottom = 5;
            style.paddingLeft = 5;
            style.borderTopLeftRadius = CORNER_RADIUS;
            style.borderTopRightRadius = CORNER_RADIUS;
            style.borderBottomLeftRadius = CORNER_RADIUS;
            style.borderBottomRightRadius = CORNER_RADIUS;
            style.backgroundColor = Color.Lerp(EditorColors.InspectorBackground, Color.black, 0.1f);

            var header = new VisualElement {
                style = {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };

            header.RegisterCallback<ClickEvent>(_ => Expanded = !Expanded);

            EditorIconsDatabase.TryGetIcon<Texture2D>(ARROW_ICON_NAME, out var arrowIcon);

            _arrow = new Image {
                image = arrowIcon,
                tintColor = EditorColors.FoldoutArrow,
                style = {
                    width = ARROW_SIZE,
                    height = ARROW_SIZE,
                    // Pushes the arrow to the right edge of the header.
                    marginLeft = StyleKeyword.Auto,
                    transitionProperty = new List<StylePropertyName> { "rotate" },
                    transitionDuration = new List<TimeValue> { new(ANIMATION_MS, TimeUnit.Millisecond) },
                    transitionTimingFunction = new List<EasingFunction> { new(EasingMode.EaseOutCubic) }
                }
            };

            Icon = new Image {
                image = icon,
                style = {
                    width = ICON_SIZE,
                    height = ICON_SIZE,
                    marginRight = 5,
                    display = icon ? DisplayStyle.Flex : DisplayStyle.None
                }
            };

            Title = new Label(title) {
                style = {
                    fontSize = 14,
                    unityTextAlign = TextAnchor.MiddleLeft
                }
            };

            header.Add(Icon);
            header.Add(Title);
            header.Add(_arrow);
            hierarchy.Add(header);

            // The clip is what grows and shrinks, through a transition of its height. The content is positioned
            // absolutely, so it keeps its full height inside it, which is the height the clip grows to.
            _clip = new VisualElement {
                style = {
                    overflow = Overflow.Hidden,
                    transitionProperty = new List<StylePropertyName> { "height" },
                    transitionTimingFunction = new List<EasingFunction> { new(EasingMode.EaseOutCubic) }
                }
            };

            _content = new VisualElement {
                style = {
                    position = Position.Absolute,
                    top = 0,
                    left = 0,
                    right = 0,
                    paddingTop = HEADER_SPACING,
                    paddingLeft = 15
                }
            };

            // Expanded, the clip follows the content, e.g. a list that gets a new entry.
            _content.RegisterCallback<GeometryChangedEvent>(_ => {
                if (_expanded)
                    _clip.style.height = _content.layout.height;
            });

            _clip.Add(_content);
            hierarchy.Add(_clip);

            Expanded = saveKey != null && EditorPrefs.GetBool(saveKey);
        }
    }
}
