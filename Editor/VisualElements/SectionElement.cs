using UnityEngine;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.VisualElements
{
    /// <summary>
    /// A group of fields under a title with an optional icon, on a background a shade darker than the inspector.
    /// <see cref="VisualElement.Add"/> puts the fields under the title.
    /// </summary>
    public class SectionElement : VisualElement
    {
        private const float ICON_SIZE = 16;
        private const float CORNER_RADIUS = 4;

        private readonly VisualElement _content;

        public Image Icon { get; }

        public Label Title { get; }

        public override VisualElement contentContainer => _content;

        public SectionElement(string title, Texture icon = null)
        {
            style.marginTop = 10;
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
                    alignItems = Align.Center,
                    marginBottom = 5,
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
            hierarchy.Add(header);

            _content = new VisualElement() {
                style = {
                    paddingLeft = 15
                }
            };
            
            hierarchy.Add(_content);
        }
    }
}
