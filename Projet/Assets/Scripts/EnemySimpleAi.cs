using UnityEngine;

public class EnemySimpleAi : MonoBehaviour
{
    [Header("Déplacement")]
    public float walkSpeed = 2f;
    public float chaseSpeed = 4f;
    
    [Header("Détection")]
    public float detectionRadius = 8f;   
    public float loseRadius = 12f;       

    private Transform player;
    private Rigidbody rb;
    private GravityBody gravityBody;
    private bool isChasing = false;
    
    // pour prendre le plan local
    private Vector3 directionPatrouilleMonde;
    private float timerPatrouille;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityBody = GetComponent<GravityBody>();
        rb.sleepThreshold = 0.0f; 

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null) player = playerObj.transform;

        ChoisirNouvelleDirection();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Système d'états Chasse / Patrouille
        if (!isChasing && distanceToPlayer <= detectionRadius)
        {
            isChasing = true; 
        }
        else if (isChasing && distanceToPlayer > loseRadius)
        {
            isChasing = false; 
            ChoisirNouvelleDirection();
        }

        // Chrono de la patrouille
        if (!isChasing)
        {
            timerPatrouille -= Time.deltaTime;
            if (timerPatrouille <= 0)
            {
                ChoisirNouvelleDirection();
            }
        }
    }

    void FixedUpdate()
    {
        if (isChasing)
        {
            ChasserJoueur();
        }
        else
        {
            Patrouiller();
        }
    }

    void ChoisirNouvelleDirection()
    {

        Vector3 directionAleatoire = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        
        // pour plaquer sur le sol
        directionPatrouilleMonde = Vector3.ProjectOnPlane(directionAleatoire, transform.up).normalized;
        
        // Temps de marche aléatoire : entre 3 et 6 secondes
        timerPatrouille = Random.Range(3f, 6f);
    }

    void Patrouiller()
    {
        // pour rester plaquer sur le sol
        Vector3 directionAjustee = Vector3.ProjectOnPlane(directionPatrouilleMonde, transform.up).normalized;

        if (directionAjustee != Vector3.zero)
        {
            // Vitesse de marche
            Vector3 vel = directionAjustee * walkSpeed;
            vel += Vector3.Project(rb.linearVelocity, transform.up); // Conserve la gravité
            rb.linearVelocity = vel;

            OrienterVers(directionAjustee);
        }
    }

    void ChasserJoueur()
    {
        Vector3 directionVersJoueur = (player.position - transform.position);
        Vector3 directionAuSol = Vector3.ProjectOnPlane(directionVersJoueur, transform.up).normalized;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > 1.5f) 
        {
            Vector3 vel = directionAuSol * chaseSpeed;
            vel += Vector3.Project(rb.linearVelocity, transform.up); 
            rb.linearVelocity = vel;
            
            OrienterVers(directionAuSol);
        }
        else
        {
            // Stop au contact du joueur
            rb.linearVelocity = Vector3.Project(rb.linearVelocity, transform.up);
        }
    }

    void OrienterVers(Vector3 directionTarget)
    {
        if (directionTarget != Vector3.zero)
        {
            Vector3 gravityDir = (gravityBody != null) ? gravityBody.GravityDirection : -transform.up;
            if (gravityDir == Vector3.zero) gravityDir = -transform.up;

            // allignement avec le sol
            Quaternion targetRot = Quaternion.LookRotation(directionTarget, -gravityDir);
            
            // rotation fluide
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRadius);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // On vérifie si on touche le joueur
        if (collision.gameObject.name == "Player")
        {
            // On cherche le script de santé
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // applique les dégâts
                playerHealth.TakeDamage(1);
            }
        }
    }
}