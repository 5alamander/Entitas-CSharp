namespace Entitas {
    
    public class PoolEntityIndexDoesAlreadyExistException : EntitasException {

        public PoolEntityIndexDoesAlreadyExistException(IPool pool, string name) :
            base("Cannot add EntityIndex '" + name + "' to pool '" + pool + "'!",
                 "An EntityIndex with this name has already been added.") {
        }
    }
}
