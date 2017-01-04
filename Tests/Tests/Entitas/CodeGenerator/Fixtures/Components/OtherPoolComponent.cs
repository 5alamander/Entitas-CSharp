using System;
using Entitas;
using Entitas.CodeGenerator;

[SingleEntity, Context("Other")]
public class OtherPoolComponent : IComponent {
    public DateTime timestamp;
    public bool isLoggedIn;
}
