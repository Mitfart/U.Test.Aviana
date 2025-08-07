using UnityEngine;

namespace Utils
{
    [RequireComponent(typeof(TypeWriter))]
    public class TypeWriterLoop : MonoBehaviour
    {
        [SerializeField] private TypeWriter typeWriter;

        
        private void OnEnable() => typeWriter.OnEndTyping += Loop;
        private void OnDisable() => typeWriter.OnEndTyping -= Loop;

        
        private void Loop()
        {
            typeWriter.StartTyping();
        }
    }
}