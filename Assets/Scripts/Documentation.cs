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

    public class Documentation : EditorWindow
    {
        private event Action OnSelectionChange = delegate { };

        private List<TypeNDescription> _descriptions;

        private List<string> _selectedParameters;

        private HashSet<string> _existedNames;

        private ListView _descriptionListView;

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

            var keysList = _existedNames.ToList();

            #region InitLeftSide

            var namesList = new ListView(keysList,
                                         makeItem: CreateLeftSideToggle,
                                         bindItem: (element, i) => (element as ToolbarToggle).text = keysList[i]);

            namesList.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            namesList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;

            #endregion

            #region InitRightSide

            GetNameParameters();

            DrawInformationList();

            OnSelectionChange += GetNameParameters;

            OnSelectionChange += ResetDescriptionList;

            #endregion

            leftSide.Add(namesList);

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

        /// <summary>
        /// Initialize needed parameters
        /// </summary>
        private void UpdateDocumentation()
        {
            _selectedParameters = new List<string>();
            _descriptions = new List<TypeNDescription>();
            _existedNames = new HashSet<string>();

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
                        _existedNames.Add(name);
                    }
                  
                    descriptionObject.AssociatedNames = documentation.Names.ToList();
                    descriptionObject.Description = documentation.Description;
                }
                
            }
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
            var toggle = evt.target as ToolbarToggle;
            
            if (evt.newValue)
            {
                toggle.style.backgroundColor = new StyleColor(new Color(0.22f, 0.22f, 0.22f));
                
                _selectedParameters.Add(toggle.text);
                
                OnSelectionChange();
                return;
            }
            
            toggle.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
            
            _selectedParameters.Remove(toggle.text);

            OnSelectionChange();
        }

        private class TypeNDescription
        {
            public Type Type;

            public List<string> AssociatedNames = new List<string>();

            public string Description;
        }

    }
}
