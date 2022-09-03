import { Component, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';
import { SharedDataService } from 'src/app/services/utils/shared-data.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-pdfviewer',
  templateUrl: './pdfviewer.component.html',
  styleUrls: ['./pdfviewer.component.scss']
})
export class PDFViewerComponent implements OnInit {
  url;
  cerrar = false;
  sharedObject: any;

  constructor(private SharedData: SharedDataService, private dialogRef: MatDialogRef<PDFViewerComponent>) { }

  ngOnInit(): void {
    this.leerData();
  }

  leerData() {
    this.SharedData.shared$.subscribe(result => {
      if (result && result?.type === 'pdf') {
        this.sharedObject = result;

        if (result?.type === 'pdf') {
          this.url = result.value;
        }
      }
    })
  }

  closeDialog() {
    this.dialogRef.close();
  }

  ngOnDestroy() {
  }
}
