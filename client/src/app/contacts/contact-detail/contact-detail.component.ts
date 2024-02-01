import { Component, Input, OnInit } from '@angular/core';
import { Contact } from 'src/app/_models/contact.model';

@Component({
  selector: 'app-contact-detail',
  templateUrl: './contact-detail.component.html',
  styleUrls: ['./contact-detail.component.css'],
})
export class ContactDetailComponent implements OnInit {
  @Input() contact: Contact | undefined;

  constructor() {}

  ngOnInit(): void {}

  startChat() {}
}
