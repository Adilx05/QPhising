import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { StepsModule } from 'primeng/steps';
import { TagModule } from 'primeng/tag';

import { SetupPageComponent } from './containers/setup-page.component';
import { SetupRoutingModule } from './setup-routing.module';
import { SetupWizardPageComponent } from './containers/setup-wizard-page.component';
import { SetupWizardShellComponent } from './components/setup-wizard-shell.component';

@NgModule({
  declarations: [SetupPageComponent, SetupWizardPageComponent, SetupWizardShellComponent],
  imports: [CommonModule, SetupRoutingModule, CardModule, ButtonModule, TagModule, StepsModule]
})
export class SetupModule {}
