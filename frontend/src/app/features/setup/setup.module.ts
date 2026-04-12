import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { StepsModule } from 'primeng/steps';
import { TagModule } from 'primeng/tag';
import { ReactiveFormsModule } from '@angular/forms';

import { SetupPageComponent } from './containers/setup-page.component';
import { SetupRoutingModule } from './setup-routing.module';
import { SetupWizardPageComponent } from './containers/setup-wizard-page.component';
import { SetupWizardShellComponent } from './components/setup-wizard-shell.component';

@NgModule({
  declarations: [SetupPageComponent, SetupWizardPageComponent, SetupWizardShellComponent],
  imports: [CommonModule, ReactiveFormsModule, SetupRoutingModule, CardModule, ButtonModule, TagModule, StepsModule, InputTextModule, PasswordModule]
})
export class SetupModule {}
