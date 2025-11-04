using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace SmokeExpress.Web.Shared.FormControls;

public abstract class FormInputBase<TValue> : ComponentBase, IDisposable
{
    [CascadingParameter]
    protected EditContext? EditContext { get; set; }

    [Parameter, EditorRequired]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public string? InputId { get; set; }
        = null;

    [Parameter]
    public string? Hint { get; set; }
        = null;

    [Parameter]
    public string? CssClass { get; set; }
        = null;

    [Parameter]
    public Expression<Func<TValue>>? ValueExpression { get; set; }
        = null;

    protected FieldIdentifier FieldIdentifier;
    protected bool HasFieldIdentifier;
    private readonly string _generatedId = $"field_{Guid.NewGuid():N}";

    private EventHandler<ValidationStateChangedEventArgs>? _validationHandler;
    private EditContext? _previousEditContext;

    protected string ResolvedInputId => !string.IsNullOrWhiteSpace(InputId)
        ? InputId!
        : HasFieldIdentifier
            ? FieldIdentifier.FieldName
            : _generatedId;

    protected string? CurrentError => HasFieldIdentifier && EditContext is not null
        ? EditContext.GetValidationMessages(FieldIdentifier).FirstOrDefault()
        : null;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (EditContext != _previousEditContext)
        {
            if (_previousEditContext is not null && _validationHandler is not null)
            {
                _previousEditContext.OnValidationStateChanged -= _validationHandler;
            }

            _previousEditContext = EditContext;

            if (EditContext is not null)
            {
                _validationHandler ??= (_, __) => InvokeAsync(StateHasChanged);
                EditContext.OnValidationStateChanged += _validationHandler;
            }
        }

        HasFieldIdentifier = false;
        if (ValueExpression is not null)
        {
            FieldIdentifier = FieldIdentifier.Create(ValueExpression);
            HasFieldIdentifier = true;
        }
    }

    protected void NotifyFieldChanged()
    {
        if (HasFieldIdentifier && EditContext is not null)
        {
            EditContext.NotifyFieldChanged(FieldIdentifier);
        }
    }

    public virtual void Dispose()
    {
        if (_previousEditContext is not null && _validationHandler is not null)
        {
            _previousEditContext.OnValidationStateChanged -= _validationHandler;
        }
    }
}

