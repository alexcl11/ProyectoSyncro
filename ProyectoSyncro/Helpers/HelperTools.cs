namespace ProyectoSyncro.Helpers
{
    public class HelperTools
    {
        //IMPORTANTE, NUESTRO SALT DEBE SER EXACTO AL TAMAÑO DEL CAMPO DE LA BD
        public static string GenerateSalt()
        {
            Random random = new Random();
            string salt = "";
            for (int i = 0; i <= 50; i++)
            {
                int num = random.Next(1, 255);
                char letra = Convert.ToChar(num);
                salt += letra;
            }
            return salt;
        }

        public static bool CompareArrays(byte[] a, byte[] b)
        {
            bool iguales = true;

            if (a.Length != b.Length)
            {
                iguales = false;
            }
            else
            {
                // COMPARAMOS BYTE A BYTE
                for (int i = 0; i < a.Length; i++)
                {
                    if (!a[i].Equals(b[i]))
                    {
                        iguales = false;
                        break;
                    }
                }
            }

            return iguales;
        }
    }
}
