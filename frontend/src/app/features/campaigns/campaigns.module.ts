import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CampaignsRoutingModule } from './campaigns-routing.module';
import { SharedModule } from '../../shared/shared.module';
import { CampaignsPageComponent } from './containers/campaigns-page.component';
import { CampaignsTableComponent } from './components/campaigns-table.component';

@NgModule({
  declarations: [CampaignsPageComponent, CampaignsTableComponent],
  imports: [CommonModule, SharedModule, CampaignsRoutingModule]
})
export class CampaignsModule {}
