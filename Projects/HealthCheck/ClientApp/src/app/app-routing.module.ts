import { NgModule } from '@angular/core';
import { Routes, RouterModule, Router } from '@angular/router';
import { HealthCheckComponent } from './health-check/health-check.component';
import { HomeComponent } from './home/home.component';

const routes: Routes = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  { path: 'health-check', component: HealthCheckComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { relativeLinkResolution: 'legacy' })],
  exports: [RouterModule]
})

export class AppRoutingModule { }
