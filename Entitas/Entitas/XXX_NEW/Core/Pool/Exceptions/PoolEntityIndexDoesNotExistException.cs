namespace Entitas {
    
    public class PoolEntityIndexDoesNotExistException : EntitasException {

        public PoolEntityIndexDoesNotExistException(IPool pool, string name) :
            base("Cannot get EntityIndex '" + name + "' from pool '" +
                 pool + "'!", "No EntityIndex with this name has been added.") {
        }
    }
}
