import { Component, input } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  template: `<div class="spinner-wrap" role="status" [attr.aria-label]="label()"><span class="loading-spinner"></span><span class="sr-only">{{ label() }}</span></div>`,
  styles: [`.spinner-wrap{display:inline-grid;place-items:center}.sr-only{position:absolute;width:1px;height:1px;padding:0;margin:-1px;overflow:hidden;clip:rect(0,0,0,0);white-space:nowrap;border:0}`]
})
export class LoadingSpinnerComponent { readonly label = input('Cargando'); }

