import { Component, Input } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-page-header',
  templateUrl: './page-header.component.html'
})
export class PageHeaderComponent {
  @Input({ required: true }) title!: string;
  @Input({ required: true }) description!: string;
}
