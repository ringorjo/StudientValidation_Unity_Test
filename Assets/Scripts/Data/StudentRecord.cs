[System.Serializable]
public class StudentRecord
{
    public string Nombre;
    public string Apellido;
    public string Codigo;
    public string Correo;
    public float NotaFinal;

    public string NombreCompleto => $"{Nombre} {Apellido}";
}
