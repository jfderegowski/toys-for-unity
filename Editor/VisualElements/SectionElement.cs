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
    public class SectionElement : FoldoutElement
    {
        private const float ARROW_SIZE = 12;
        private const float CORNER_RADIUS = 4;
        private const float HEADER_SPACING = 5;
        private const int ANIMATION_MS = 150;
        private const string ARROW_ICON_NAME = "DropdownArrow";

        private readonly string _saveKey;

        private Image _arrow;
        private VisualElement _clip;
        private VisualElement _content;

        public Label Title { get; private set; }

        /// <summary>
        /// Whether the fields are shown, the <see cref="Foldout.value"/>. With a save key it is kept in the
        /// <see cref="EditorPrefs"/>.
        /// </summary>
        public bool Expanded
        {
            get => value;
            set => this.value = value;
        }

        /// <param name="title">Text of the header.</param>
        /// <param name="icon">Icon before the title; none when null.</param>
        /// <param name="saveKey">
        /// <see cref="EditorPrefs"/> key the collapsed state is kept under, e.g. the type and the title, so it stays
        /// when the inspector is rebuilt; null keeps it only while this element lives.
        /// </param>
        public SectionElement(string title, Texture icon = null, string saveKey = null)
            : base(title, icon: icon)
        {
            _saveKey = saveKey;

            SetValueWithoutNotify(saveKey != null && EditorPrefs.GetBool(saveKey));
            Build();
        }

        /// <summary>
        /// The property under its name, with its fields. Its collapsed state is the one of the property.
        /// </summary>
        /// <param name="property">Property</param>
        /// <param name="icon">Icon before the name; none when null.</param>
        public SectionElement(SerializedProperty property, Texture icon = null)
            : base(property, icon: icon)
        {
            Build();
        }

        private void Build()
        {
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

            // The header takes the place of the toggle of the foldout, without its inspector margins.
            var toggle = Input.parent;

            toggle.style.marginTop = 0;
            toggle.style.marginRight = 0;
            toggle.style.marginBottom = 0;
            toggle.style.marginLeft = 0;
            Input.style.alignItems = Align.Center;

            // The arrow of the foldout gives way to the one at the right edge.
            Input.Q(className: checkmarkUssClassName).style.display = DisplayStyle.None;

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

            Input.Add(_arrow);

            Title = Input.Q<Label>(className: textUssClassName);

            // Spaced as a plain label, not as the text of a foldout.
            if (Title != null)
            {
                Title.style.marginLeft = 0;
                Title.style.paddingTop = 0;
                Title.style.paddingLeft = 1;
                Title.style.fontSize = 14;
                Title.style.unityTextAlign = TextAnchor.MiddleLeft;
            }

            // The clip is what grows and shrinks, through a transition of its height. The content is positioned
            // absolutely, so it keeps its full height inside it, which is the height the clip grows to.
            _clip = new VisualElement {
                style = {
                    overflow = Overflow.Hidden,
                    transitionProperty = new List<StylePropertyName> { "height" },
                    transitionTimingFunction = new List<EasingFunction> { new(EasingMode.EaseOutCubic) }
                }
            };

            _content = contentContainer;
            _content.style.position = Position.Absolute;
            _content.style.top = 0;
            _content.style.left = 0;
            _content.style.right = 0;
            _content.style.marginLeft = 0;
            _content.style.paddingTop = HEADER_SPACING;
            _content.style.paddingLeft = 15;

            // The foldout hides the content when it collapses, which shows and hides it as the content's geometry
            // changes; the clip and the arrow follow it there. Expanded, the clip also follows the content, e.g. a
            // list that gets a new entry.
            _content.RegisterCallback<GeometryChangedEvent>(_ => Refresh());

            // A click or Expanded animates, keeping the content shown while the clip closes over it.
            RegisterCallback<ChangeEvent<bool>>(evt => {
                // A toggle among the fields.
                if (evt.target != this)
                    return;

                _clip.style.transitionDuration = new List<TimeValue> { new(ANIMATION_MS, TimeUnit.Millisecond) };
                _content.style.display = DisplayStyle.Flex;
                Refresh();

                if (_saveKey != null)
                    EditorPrefs.SetBool(_saveKey, evt.newValue);
            });

            _clip.Add(_content);
            hierarchy.Add(_clip);

            Refresh();
        }

        private void Refresh()
        {
            _arrow.style.rotate = new Rotate(new Angle(value ? 0 : -90, AngleUnit.Degree));

            // Not laid out yet: expanded, it opens at once to the height of the content, through its
            // GeometryChangedEvent.
            _clip.style.height = value && panel != null ? _content.layout.height : 0;
        }
    }
}
