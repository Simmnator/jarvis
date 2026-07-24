import argparse
import yt_dlp
import sys

def download_video(url, output_path=None):
    """
    Lädt ein Video von der angegebenen URL (z.B. YouTube, TikTok) in bester Qualität herunter.
    """
    ydl_opts = {
        'format': 'bestvideo+bestaudio/best',
        'outtmpl': '%(title)s.%(ext)s',
        'noplaylist': True,
        'quiet': False,
    }

    if output_path:
        ydl_opts['outtmpl'] = f'{output_path}/%(title)s.%(ext)s'

    try:
        with yt_dlp.YoutubeDL(ydl_opts) as ydl:
            print(f"Starte Download von: {url}...")
            ydl.download([url])
            print("Download erfolgreich abgeschlossen!")
    except Exception as e:
        print(f"Fehler beim Herunterladen: {e}")
        sys.exit(1)

def main():
    parser = argparse.ArgumentParser(description="Ein einfaches Tool zum Herunterladen von Videos (YouTube, TikTok, etc.) in bester Qualität.")
    parser.add_argument("url", help="Die URL des Videos, das heruntergeladen werden soll.")
    parser.add_argument("-o", "--output", help="Optional: Pfad zum Ausgabeordner.", default=None)

    args = parser.parse_args()

    download_video(args.url, args.output)

if __name__ == "__main__":
    main()
