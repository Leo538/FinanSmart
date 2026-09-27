import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

type AnalysisTab = 'credito' | 'comparacion' | 'inversion';

interface AnalysisRow {
  number: string;
  name: string;
  tag: string;
  detail: string;
  result: string;
  insight: string;
  direction: 'up' | 'down';
}

@Component({
  selector: 'app-public-home',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './public-home.component.html',
  styleUrl: './public-home.component.scss'
})
export class PublicHomeComponent {
  activeAnalysis: AnalysisTab = 'comparacion';

  readonly analysis: Record<AnalysisTab, { title: string; description: string; rows: AnalysisRow[] }> = {
    credito: {
      title: 'Cada detalle de tu crédito, a la vista',
      description: 'Explora los componentes que intervienen en una simulación antes de tomar una decisión.',
      rows: [
        { number: '01', name: 'Monto solicitado', tag: 'Capital', detail: 'Valor a financiar', result: 'Editable', insight: 'Base de cálculo', direction: 'up' },
        { number: '02', name: 'Plazo', tag: 'Tiempo', detail: 'Número de cuotas', result: 'Editable', insight: 'Impacta la cuota', direction: 'down' },
        { number: '03', name: 'Tasa', tag: 'Interés', detail: 'Condición aplicable', result: 'Visible', insight: 'Costo financiero', direction: 'up' },
        { number: '04', name: 'Cargos', tag: 'Costos', detail: 'Conceptos adicionales', result: 'Desglosados', insight: 'Costo total', direction: 'down' },
        { number: '05', name: 'Cuota', tag: 'Resultado', detail: 'Pago periódico', result: 'Calculada', insight: 'Planifica pagos', direction: 'up' },
        { number: '06', name: 'Amortización', tag: 'Detalle', detail: 'Capital e intereses', result: 'Por periodo', insight: 'Sigue el saldo', direction: 'up' }
      ]
    },
    comparacion: {
      title: 'Compara sistemas de amortización',
      description: 'Ve las diferencias entre el sistema Francés y el Alemán en un formato fácil de recorrer.',
      rows: [
        { number: '01', name: 'Sistema Francés', tag: 'Cuotas', detail: 'Pago periódico constante', result: 'Comparar', insight: 'Cuota estable', direction: 'up' },
        { number: '02', name: 'Sistema Alemán', tag: 'Cuotas', detail: 'Abono a capital constante', result: 'Comparar', insight: 'Cuota decreciente', direction: 'down' },
        { number: '03', name: 'Intereses', tag: 'Costo', detail: 'Cobro por periodo', result: 'Desglosado', insight: 'Revisa diferencias', direction: 'down' },
        { number: '04', name: 'Abono a capital', tag: 'Pago', detail: 'Reducción del saldo', result: 'Por cuota', insight: 'Sigue el avance', direction: 'up' },
        { number: '05', name: 'Saldo pendiente', tag: 'Balance', detail: 'Capital por pagar', result: 'Actualizado', insight: 'Control del crédito', direction: 'down' },
        { number: '06', name: 'Costo total', tag: 'Resumen', detail: 'Valor final proyectado', result: 'Cara a cara', insight: 'Decisión informada', direction: 'up' }
      ]
    },
    inversion: {
      title: 'Analiza el potencial de tu inversión',
      description: 'Consulta las variables de una proyección y entiende cómo se construye el resultado.',
      rows: [
        { number: '01', name: 'Capital inicial', tag: 'Aporte', detail: 'Monto a invertir', result: 'Editable', insight: 'Punto de partida', direction: 'up' },
        { number: '02', name: 'Plazo', tag: 'Tiempo', detail: 'Duración prevista', result: 'Editable', insight: 'Horizonte', direction: 'up' },
        { number: '03', name: 'Tasa', tag: 'Retorno', detail: 'Condición del producto', result: 'Visible', insight: 'Proyección', direction: 'up' },
        { number: '04', name: 'Rendimiento', tag: 'Ganancia', detail: 'Resultado estimado', result: 'Calculado', insight: 'Evolución', direction: 'up' },
        { number: '05', name: 'Valor final', tag: 'Total', detail: 'Capital más rendimiento', result: 'Calculado', insight: 'Meta financiera', direction: 'up' },
        { number: '06', name: 'Solicitud', tag: 'Acción', detail: 'Siguiente paso', result: 'Disponible', insight: 'Cuando decidas', direction: 'up' }
      ]
    }
  };

  selectAnalysis(tab: AnalysisTab): void {
    this.activeAnalysis = tab;
  }
}
