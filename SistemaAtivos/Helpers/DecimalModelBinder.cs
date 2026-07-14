using System;
using System.Globalization;
using System.Web.Mvc;

namespace SistemaAtivos.Helpers
{
    /// <summary>
    /// Model binder customizado para decimal que aceita tanto ponto quanto vírgula
    /// como separador decimal, independente da cultura configurada no servidor.
    /// Resolve o conflito entre cultura pt-BR (vírgula) e valores enviados pelo JS (ponto).
    /// </summary>
    public class DecimalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (valueResult == null)
                return null;

            var rawValue = valueResult.AttemptedValue;
            if (string.IsNullOrWhiteSpace(rawValue))
                return null;

            // Normaliza: remove espaços, troca ponto de milhar, garante ponto decimal
            // Suporta: "7.000,50" (BR), "7000.50" (invariant), "7000,50", "7000"
            var normalized = rawValue.Trim();

            // Caso BR: tem vírgula como decimal (ex: 7.000,50 ou 7000,50)
            if (normalized.Contains(","))
            {
                // Remove pontos de milhar e troca vírgula por ponto
                normalized = normalized.Replace(".", "").Replace(",", ".");
            }
            // Caso invariant: tem ponto como decimal (ex: 7000.50) — já está correto

            if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;

            bindingContext.ModelState.AddModelError(
                bindingContext.ModelName,
                $"O valor '{rawValue}' não é válido para {bindingContext.ModelMetadata.DisplayName ?? bindingContext.ModelName}.");
            return null;
        }
    }
}
