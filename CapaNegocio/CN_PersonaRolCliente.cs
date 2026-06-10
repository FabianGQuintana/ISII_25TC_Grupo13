using System;

namespace CapaNegocio
{
    // Clase abstracta del diagrama: PersonaRolCliente.
    // Define el contrato polimórfico; cada rol especializa (override) la obtención.
    public abstract class CN_PersonaRolCliente<T> where T : class
    {
        public abstract T? ObtenerPorId(Guid id);
        public abstract T? ObtenerPorDni(string dni);
    }
}
