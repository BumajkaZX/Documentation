namespace Documentation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Эдиторское окно документации
    /// </summary>
    public class Documentation : EditorWindow
    {
        private event Action OnSelectionChange = delegate { };

        private List<TypeNDescription> _descriptions;

        private List<string> _selectedParameters;

        private List<string> _existedNames;

        private List<string> _filteredNames;

        private ListView _descriptionListView;

        private ListView _namesListView;

        private List<TypeNDescription> _information;

        [MenuItem("Documentation/Open")]
        public static void OpenWindow()
        {
            var docWindow = GetWindow<Documentation>();
            docWindow.Show();
        }

        private void CreateGUI()
        {
            UpdateDocumentation();

            var root = rootVisualElement;

            var toolbar = new Toolbar();

            var splitView = new TwoPaneSplitView(0, 100, TwoPaneSplitViewOrientation.Horizontal);

            var leftSide = new VisualElement();
            leftSide.style.minWidth = 100;
            leftSide.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            
            var rightSide = new VisualElement();
            rightSide.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            rightSide.style.minWidth = 100;
            
            var searchTab = new ToolbarSearchField();
            searchTab.RegisterValueChangedCallback(LeftTabCallback);
            
            #region InitLeftSide

            DrawNamesList();

            #endregion

            #region InitRightSide

            GetNameParameters();

            DrawInformationList();

            OnSelectionChange += GetNameParameters;

            OnSelectionChange += ResetDescriptionList;

            #endregion
            
            toolbar.Add(searchTab);
            
            leftSide.Add(_namesListView);

            rightSide.Add(_descriptionListView);

            splitView.Add(leftSide);
            splitView.Add(rightSide);

            root.Add(toolbar);
            root.Add(splitView);

            void DrawInformationList()
            {
                if (_descriptionListView == null)
                {
                    _descriptionListView = new ListView(_information,
                                                        makeItem: CreateRightSideInfoBox,
                                                        bindItem: (element, i) =>
                                                        {
                                                            SetInformationToBox(element, _information[i]);
                                                        });

                    _descriptionListView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
                    _descriptionListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
                }
            }

            void DrawNamesList()
            {
                if (_namesListView == null)
                {
                    _namesListView = new ListView(_filteredNames,
                                                  makeItem: CreateLeftSideToggle,
                                                  bindItem: (element, i) =>
                                                  {
                                                      var toggle = (ToolbarToggle)element;
                                                      bool isSelected = _selectedParameters.Contains(_filteredNames[i]);
                                                      
                                                      toggle.text = _filteredNames[i];
                                                      SetToggle(toggle, isSelected);
                                                      SetToggleValue(toggle, isSelected);
                                                  });

                    _namesListView.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
                    _namesListView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
                }
            }
        }
        private void LeftTabCallback(ChangeEvent<string> filterName)
        {
            if (string.IsNullOrEmpty(filterName.newValue))
            {
                _filteredNames = _existedNames;
                _namesListView.itemsSource = _filteredNames;
                return;
            }
            
            _filteredNames = _existedNames.Where(name => name.Contains(filterName.newValue, StringComparison.CurrentCultureIgnoreCase)).ToList();
            _filteredNames.Sort();
            _namesListView.itemsSource = _filteredNames;
        }
        
        private void ResetDescriptionList()
        {
            _descriptionListView.itemsSource = _information;
        }

        private void GetNameParameters()
        {
            _information = new List<TypeNDescription>();
            
            if (_selectedParameters.Count == 0)
            {
                _information = _descriptions;
                
                return;
            }
            
            var hashToAdd = new HashSet<TypeNDescription>();
            
            foreach (var typeNDescription in _descriptions)
            {
                foreach (var key in _selectedParameters)
                {
                    if (!typeNDescription.AssociatedNames.Contains(key))
                    {
                        hashToAdd.Clear();
                        break;
                    }
                    
                    hashToAdd.Add(typeNDescription);
                }
                
                _information.AddRange(hashToAdd);
                hashToAdd.Clear();
            }
        }
        
        private void UpdateDocumentation()
        {
            _selectedParameters = new List<string>();
            _descriptions = new List<TypeNDescription>();
            _existedNames = new List<string>();

            var names = new HashSet<string>();

            var assembly = Assembly.GetExecutingAssembly();

            var types = assembly.GetTypes()
                .Where(type => type.GetCustomAttributes(typeof(DocumentationAttribute), false).Any())
                .ToList();
            
            foreach (var type in types)
            {
                var descriptionObject = new TypeNDescription()
                {
                    Type = type
                };
                _descriptions.Add(descriptionObject);
                
                var attributes = type.GetCustomAttributes(typeof(DocumentationAttribute), true);
                
                if (attributes[0] is DocumentationAttribute documentation)
                {
                    foreach (var name in documentation.Names)
                    {
                        names.Add(name);
                    }
                  
                    descriptionObject.AssociatedNames = documentation.Names.ToList();
                    descriptionObject.Description = documentation.Description;
                }
                
            }

            _existedNames = names.ToList();
            _existedNames.Sort();
            _filteredNames = _existedNames;
        }

        private VisualElement CreateLeftSideToggle()
        {
            var toggle = new ToolbarToggle();
            toggle.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
            toggle.RegisterValueChangedCallback(OnClassInfoClicked);
            return toggle;
        }

        private void SetInformationToBox(VisualElement boxElement, TypeNDescription typeNDescription)
        {
            var root = boxElement;

            var leftRoot = root.ElementAt(0);

            var rightRoot = root.ElementAt(1);

            var classInfo = leftRoot.ElementAt(0) as TextElement;

            classInfo.text = $"{typeNDescription.Type.FullName} : {String.Join(", ", typeNDescription.AssociatedNames)}";

            var description = leftRoot.ElementAt(1) as TextElement;

            description.text = $"{typeNDescription.Description}";
        }

        private VisualElement CreateRightSideInfoBox()
        {
            var root = new VisualElement();
            root.style.backgroundColor = new Color(0.17f, 0.17f, 0.17f);
            
            var splitUp = new VisualElement();
            splitUp.style.paddingLeft = 5;
            var splitMiddle = new VisualElement();
            splitMiddle.style.paddingLeft = 5;

            var splitBottom = new VisualElement();
            splitBottom.style.minHeight = 20; //Space btw
            splitBottom.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            var classInfo = new TextElement();
            classInfo.style.paddingBottom = 5;
            classInfo.style.fontSize = 13;

            var description = new TextElement();
            
            splitUp.Add(classInfo);
            splitUp.Add(description);
            
            root.Add(splitUp);
            root.Add(splitMiddle);
            root.Add(splitBottom);

            return root;
        }
        private void OnClassInfoClicked(ChangeEvent<bool> evt)
        {
            _namesListView.ClearSelection();
            
            var toggle = evt.target as ToolbarToggle;

            if (evt.newValue)
            {
                _selectedParameters.Add(toggle.text);
            }
            else
            {
                _selectedParameters.Remove(toggle.text);
            }
            
            SetToggle(toggle, evt.newValue);
        }

        private void SetToggle(ToolbarToggle toggle, bool value)
        {
            OnSelectionChange();
            if (value)
            {
                toggle.style.backgroundColor = new StyleColor(new Color(0.22f, 0.22f, 0.22f));
                return;
            }
            
            toggle.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
        }

        private void SetToggleValue(ToolbarToggle toggle, bool value) => toggle.SetValueWithoutNotify(value);

        private class TypeNDescription
        {
            public Type Type;

            public List<string> AssociatedNames = new List<string>();

            public string Description;
        }

    }
}
