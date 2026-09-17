import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './layouts/admin-layout/admin-layout.component';
import { AuthLayoutComponent } from './layouts/auth-layout/auth-layout.component';
import { ClientLayoutComponent } from './layouts/client-layout/client-layout.component';
import { AdminDashboardComponent } from './features/admin/dashboard/admin-dashboard.component';
import { ClientDashboardComponent } from './features/client/dashboard/client-dashboard.component';
import { LoginComponent } from './features/auth/pages/login.component';
import { PlaceholderPageComponent } from './shared/components/placeholder-page/placeholder-page.component';

const placeholder = (title: string, subtitle: string) => ({ component: PlaceholderPageComponent, data: { title, subtitle } });

export const routes: Routes = [
  { path: 'login', component: AuthLayoutComponent, children: [{ path: '', component: LoginComponent }] },
  { path: 'register', component: AuthLayoutComponent, children: [{ path: '', ...placeholder('Crear cuenta', 'Registro disponible próximamente') }] },
  { path: 'forgot-password', component: AuthLayoutComponent, children: [{ path: '', ...placeholder('Recuperar contraseña', 'Esta opción estará disponible próximamente') }] },
  { path: 'admin', component: AdminLayoutComponent, children: [
    { path: 'dashboard', component: AdminDashboardComponent },
    { path: 'institution', ...placeholder('Institución', 'Configura la información institucional') },
    { path: 'credits/types', ...placeholder('Tipos de crédito', 'Administra los productos crediticios disponibles') },
    { path: 'credits/rates', ...placeholder('Tasas', 'Gestiona las tasas de interés configuradas') },
    { path: 'credits/charges', ...placeholder('Cargos', 'Gestiona los cargos adicionales de cada crédito') },
    { path: 'credits/simulator', ...placeholder('Simulador de crédito', 'Calcula escenarios de crédito') },
    { path: 'credits/comparison', ...placeholder('Comparación de sistemas', 'Compara sistemas de amortización') },
    { path: 'investments/products', ...placeholder('Productos de inversión', 'Módulo en preparación') },
    { path: 'investments/rates', ...placeholder('Tasas de inversión', 'Módulo en preparación') },
    { path: 'clients', ...placeholder('Clientes', 'Gestiona la información de clientes') },
    { path: 'reports', ...placeholder('Reportes', 'Consulta reportes de la plataforma') },
    { path: 'users', ...placeholder('Usuarios', 'Gestiona usuarios del sistema') },
    { path: '', pathMatch: 'full', redirectTo: 'dashboard' }
  ] },
  { path: 'client', component: ClientLayoutComponent, children: [
    { path: 'dashboard', component: ClientDashboardComponent },
    { path: 'credits/simulator', ...placeholder('Simular un crédito', 'Calcula tus cuotas próximamente') },
    { path: 'credits/comparison', ...placeholder('Comparar sistemas', 'Compara alternativas de amortización') },
    { path: 'investments/simulator', ...placeholder('Simular una inversión', 'Módulo en preparación') },
    { path: 'simulations', ...placeholder('Mis simulaciones', 'Aquí verás tus simulaciones guardadas') },
    { path: 'investments', ...placeholder('Mis inversiones', 'Aquí verás tus inversiones') },
    { path: '', pathMatch: 'full', redirectTo: 'dashboard' }
  ] },
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: '**', redirectTo: 'login' }
];
