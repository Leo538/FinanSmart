import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './layouts/admin-layout/admin-layout.component';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout.component';
import { ClientLayoutComponent } from './layouts/client-layout/client-layout.component';
import { AdminDashboardComponent } from './features/admin/dashboard/admin-dashboard.component';
import { ClientDashboardComponent } from './features/client/dashboard/client-dashboard.component';
import { LoginComponent } from './features/auth/pages/login.component';
import { PlaceholderPageComponent } from './shared/components/placeholder-page/placeholder-page.component';
import { InstitutionComponent } from './features/admin/institution/institution.component';
import { CreditTypesComponent } from './features/admin/credits/credit-types/credit-types.component';
import { CreditRatesComponent } from './features/admin/credits/credit-rates/credit-rates.component';
import { CreditChargesComponent } from './features/admin/credits/credit-charges/credit-charges.component';
import { CreditSimulatorComponent } from './features/credits/simulator/credit-simulator.component';
import { CreditComparisonComponent } from './features/credits/comparison/credit-comparison.component';
import { InvestmentProductsComponent } from './features/admin/investments/investment-products/investment-products.component';
import { InvestmentRatesComponent } from './features/admin/investments/investment-rates/investment-rates.component';
import { InvestmentSimulatorComponent } from './features/investments/simulator/investment-simulator.component';

const placeholder = (title: string, subtitle: string) => ({ component: PlaceholderPageComponent, data: { title, subtitle } });

export const routes: Routes = [
  { path: 'login', component: AuthLayoutComponent, children: [{ path: '', component: LoginComponent }] },
  { path: 'register', component: AuthLayoutComponent, children: [{ path: '', ...placeholder('Crear cuenta', 'Registro disponible próximamente') }] },
  { path: 'forgot-password', component: AuthLayoutComponent, children: [{ path: '', ...placeholder('Recuperar contraseña', 'Esta opción estará disponible próximamente') }] },
  { path: 'admin', component: AdminLayoutComponent, children: [
    { path: 'dashboard', component: AdminDashboardComponent },
    { path: 'institution', component: InstitutionComponent },
    { path: 'credits/types', component: CreditTypesComponent },
    { path: 'credits/rates', component: CreditRatesComponent },
    { path: 'credits/charges', component: CreditChargesComponent },
    { path: 'credits/simulator', component: CreditSimulatorComponent },
    { path: 'credits/comparison', component: CreditComparisonComponent },
    { path: 'investments/products', component: InvestmentProductsComponent },
    { path: 'investments/rates', component: InvestmentRatesComponent },
    { path: 'clients', ...placeholder('Clientes', 'Gestiona la información de clientes') },
    { path: 'reports', ...placeholder('Reportes', 'Consulta reportes de la plataforma') },
    { path: 'users', ...placeholder('Usuarios', 'Gestiona usuarios del sistema') },
    { path: '', pathMatch: 'full', redirectTo: 'dashboard' }
  ] },
  { path: 'client', component: ClientLayoutComponent, children: [
    { path: 'dashboard', component: ClientDashboardComponent },
    { path: 'credits/simulator', component: CreditSimulatorComponent },
    { path: 'credits/comparison', component: CreditComparisonComponent },
    { path: 'investments/simulator', component: InvestmentSimulatorComponent },
    { path: 'simulations', ...placeholder('Mis simulaciones', 'Aquí verás tus simulaciones guardadas') },
    { path: 'investments', ...placeholder('Mis inversiones', 'Aquí verás tus inversiones') },
    { path: '', pathMatch: 'full', redirectTo: 'dashboard' }
  ] },
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: '**', redirectTo: 'login' }
];
