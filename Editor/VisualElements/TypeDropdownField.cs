using System;
using System.Collections.Generic;
using System.Linq;
using fefek5.Toys.Editor.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace fefek5.Toys.Editor.VisualElements
{
    /// <summary>
    /// A dropdown of types, sorted by display name. It only picks a type: what the pick creates or replaces is up to
    /// the caller, so it serves [SerializeReference] fields and assets alike.
    /// </summary>
    public class TypeDropdownField : DropdownField
    {
        private const string NONE_CHOICE = "None";

        /// <summary>The types to pick from, in the order they are listed.</summary>
        public IReadOnlyList<Type> Types => _types;

        /// <summary>The picked type, or null for "None".</summary>
        public Type SelectedType => index > 0 ? _types[index - 1] : null;

        private readonly List<Type> _types;

        /// <param name="types">The types to pick from</param>
        /// <param name="current">Type shown at the start, or null for "None"</param>
        /// <param name="onPicked">Called with the picked type, or null when "None" is picked</param>
        public TypeDropdownField(IEnumerable<Type> types, Type current, Action<Type> onPicked)
        {
            _types = Sort(types);

            choices = _types.Select(type => type.GetDisplayName()).Prepend(NONE_CHOICE).ToList();
            SetValueWithoutNotify(current?.GetDisplayName() ?? NONE_CHOICE);

            this.RegisterValueChangedCallback(_ => onPicked?.Invoke(SelectedType));
        }

        /// <summary>
        /// Shows the same types as a menu below <paramref name="position"/>, for an add button.
        /// </summary>
        public static void ShowMenu(Rect position, IEnumerable<Type> types, Action<Type> onPicked)
        {
            var menu = new GenericMenu();

            foreach (var type in Sort(types))
                menu.AddItem(new GUIContent(type.GetDisplayName()), false, () => onPicked(type));

            if (menu.GetItemCount() == 0)
                menu.AddDisabledItem(new GUIContent("No types to pick from"));

            menu.DropDown(position);
        }

        private static List<Type> Sort(IEnumerable<Type> types) => types.OrderBy(type => type.GetDisplayName()).ToList();
    }
}
