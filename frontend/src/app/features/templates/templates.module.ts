import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SharedModule } from '../../shared/shared.module';
import { TemplatesRoutingModule } from './templates-routing.module';
import { TemplatesPageComponent } from './containers/templates-page.component';
import { TemplatesTableComponent } from './components/templates-table.component';

@NgModule({
  declarations: [TemplatesPageComponent, TemplatesTableComponent],
  imports: [CommonModule, SharedModule, TemplatesRoutingModule]
})
export class TemplatesModule {}
