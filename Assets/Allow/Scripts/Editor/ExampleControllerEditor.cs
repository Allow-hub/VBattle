using UnityEditor;

namespace Allow.EditorTools
{
   [CustomEditor(typeof(ExampleController))]
    public class ExampleControllerEditor : PolymorphicListEditor<ExampleController, IExampleBehaviour>
    {
        protected override string PropertyName => "behaviours";
    }
}
