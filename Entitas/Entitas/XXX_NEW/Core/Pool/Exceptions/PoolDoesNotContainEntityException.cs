namespace Entitas {
    
    public class PoolDoesNotContainEntityException : EntitasException {

        public PoolDoesNotContainEntityException(string message, string hint) :
            base(message + "\nPool does not contain entity!", hint) {
        }
    }
}
