import { Component } from '@angular/core';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CardModule],
  template: `
    <p-card header="Dashboard" subheader="Setup tamamlandıktan sonra ana uygulama burada çalışacak.">
      <p class="m-0 text-slate-700">
        Setup guard aktif. Backend <code>/api/setup/guard-decision</code> sonucuna göre bu sayfa erişime açılıyor.
      </p>
    </p-card>
  `
})
export class DashboardPageComponent {}
