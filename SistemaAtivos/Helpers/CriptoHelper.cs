using System.Security.Cryptography;
using System.Text;

namespace SistemaAtivos.Helpers
{
    // =====================================================================
    // REQUISITO 1 - SEGURANCA DE SENHAS (HASH SHA-256)
    // "Triturador de Senhas": converte uma senha pura em um codigo
    // embaralhado de via unica, impossivel de reverter.
    // O Hash e sempre o mesmo para a mesma entrada, o que permite
    // comparar senhas sem precisar armazenar o texto original.
    // =====================================================================
    public static class CriptoHelper
    {
        // Aplica o algoritmo SHA-256 na senha pura e retorna o Hash em formato hexadecimal
        public static string HashSHA256(string senhaPura)
        {
            if (string.IsNullOrEmpty(senhaPura)) return "";

            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Converte a string para um array de bytes
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(senhaPura));

                // Converte o array de bytes para string hexadecimal (o formato do Hash)
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
