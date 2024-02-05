import { Component, Input, OnInit } from '@angular/core';
import { ContactModel } from 'src/app/_models/contact.model';

@Component({
  selector: 'app-contact-detail',
  templateUrl: './contact-detail.component.html',
  styleUrls: ['./contact-detail.component.css'],
})
export class ContactDetailComponent implements OnInit {
  @Input() contact: ContactModel | undefined;

  constructor() {}

  ngOnInit(): void {}

  newChat() {}

  deleteContact() {}
}
