using Entitas;
using Entitas.CodeGenerator;

[Context("ServicePool"), CustomComponentName("GeneratedService")]
public class SomeService {
}

[Context("ServicePool")]
public class GeneratedService : IComponent {
    public SomeService value;
}
