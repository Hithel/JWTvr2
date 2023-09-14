
namespace APIProyecto.Helpers;
public class Authorization
{
    public enum Roles
    {
        Administrador,
        Manager,
        Employee
    }

    public const Roles rol_default = Roles.Employee;
}